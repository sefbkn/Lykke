using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract.Common;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Decred.Api.Common;
using Lykke.Service.Decred.Api.Repository;
using Lykke.Service.Decred.Api.Services;
using Microsoft.AspNetCore.Mvc;
using NDecred.Common;
using Newtonsoft.Json;
using Paymetheus.Decred;

namespace Lykke.Service.Decred.Api.Controllers
{
    public class OperationController : Controller
    {
        private readonly TransactionHistoryService _transactionHistoryService;
        private readonly TransactionBuilderService _txBuilderService;
        private readonly ITransactionBroadcastService _txBroadcastService;
        private readonly IObservableOperationRepository<KeyValueEntity> _operationRepo;

        public OperationController(
            TransactionHistoryService transactionHistoryService,
            TransactionBuilderService txBuilderService,
            ITransactionBroadcastService txBroadcastService,
            IObservableOperationRepository<KeyValueEntity> operationRepo)
        {
            _transactionHistoryService = transactionHistoryService;
            _txBuilderService = txBuilderService;
            _txBroadcastService = txBroadcastService;
            _operationRepo = operationRepo;
        }
        
        [HttpGet("api/capabilities")]
        public async Task<CapabilitiesResponse> GetCapabilities()
        {
            return new CapabilitiesResponse
            {
                AreManyInputsSupported = false,
                AreManyOutputsSupported = false,
                IsTransactionsRebuildingSupported = false
            };
        }
        
        [HttpPost("api/transactions/single")]
        public async Task<IActionResult> BuildSingleTransaction([FromBody] BuildSingleTransactionRequest request)
        {
            // Do not scale the fee
            const int feeFactor = 1;
            return await BuildTxInternal(request, feeFactor);
        }
        
        private async Task<IActionResult> BuildTxInternal(BuildSingleTransactionRequest request, decimal feeFactor)
        {            
            try
            {
                // Check to see if the request exists already.  If so, return the cached object.
                var cachedRequest = await _operationRepo.GetAsync(RecordType.UnsignedTransaction, request.OperationId.ToString());
                if (cachedRequest?.Value != null)
                    return Json(JsonConvert.DeserializeObject(cachedRequest.Value));

                var response = await _txBuilderService.BuildSingleTransactionAsync(request, feeFactor);
                
                await _operationRepo.InsertAsync(new KeyValueEntity(
                    RecordType.UnsignedTransaction,
                    request.OperationId.ToString(), 
                    JsonConvert.SerializeObject(response)));
                
                return Json(response);
            }
            
            catch (BusinessException exception) when (exception.Reason == ErrorReason.AmountTooSmall)
            {
                return Json(new
                {
                    errorCode = "amountIsTooSmall",
                    transactionContext = (string) null
                });
            }
            
            catch (BusinessException exception) when (exception.Reason == ErrorReason.NotEnoughBalance)
            {
                return Json(new
                {
                    errorCode = "notEnoughBalance",
                    transactionContext = (string) null
                });
            }
        }

        [HttpGet("api/transactions/broadcast/single/{operationId}")]
        public async Task<IActionResult> GetBroadcastedSingleTx(Guid operationId)
        {
            var cachedRequest = await _operationRepo.GetAsync(RecordType.BroadcastedTransaction, operationId.ToString());
            if (cachedRequest == null)
                return NoContent();
            
            // Retrieve the tx info, and update it.
            
            
            var broadcastRequest = JsonConvert.DeserializeObject<BroadcastedSingleTransactionResponse>(cachedRequest.Value);
            return Json(broadcastRequest);
        }
        
        [HttpPost("api/transactions/broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastTransactionRequest request)
        {
            try
            {
                // If this operationid was already broadcast, return 409 conflict.
                var cachedRequest = await _operationRepo.GetAsync(RecordType.BroadcastedTransaction, request.OperationId.ToString());
                if (cachedRequest?.Value != null)
                {
                    Response.StatusCode = (int) HttpStatusCode.Conflict;
                }
                else
                {
                    // Broadcast the signed transaction and flag the operation as broadcast
                    var result = await _txBroadcastService.Broadcast(request.OperationId, request.SignedTransaction);
                    var resultJson = JsonConvert.SerializeObject(result);
                    
                    await _operationRepo.InsertAsync(new KeyValueEntity(RecordType.BroadcastedTransaction, request.OperationId.ToString(), resultJson));
                    Response.StatusCode = (int) HttpStatusCode.OK;
                }
            }
            catch (TransactionBroadcastException e)
            {
                Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            }

            // We don't have a way to easily map the errors from dcrd to
            // amountIsTooSmall or notEnoughBalance
            return Json(new {
                errorMessage = ""
            });
        }

        [HttpDelete("api/transactions/broadcast/{operationId}")]
        public async Task<IActionResult> RemoveObservableOperation(Guid operationId)
        {
            var operation = await _operationRepo.GetAsync(RecordType.BroadcastedTransaction, operationId.ToString());
            if (operation == null)
                return NoContent();
            
            await _operationRepo.DeleteAsync(operation);
            return Ok();
        }
                
        [HttpPost("api/transactions/history/from/{address}/observation")]
        public async Task<IActionResult> SubscribeAddressFrom(string address)
        {
            try
            {
                await _transactionHistoryService.SubscribeAddressFrom(address);
                return Ok();
            }
            catch (BusinessException e) when(e.Reason == ErrorReason.DuplicateRecord)
            {
                return new StatusCodeResult(409);
            }
        }
        
        [HttpPost("api/transactions/history/to/{address}/observation")]
        public async Task<IActionResult> SubscribeAddressTo(string address)
        {
            try
            {
                await _transactionHistoryService.SubscribeAddressTo(address);
                return Ok();
            }
            catch (BusinessException e) when(e.Reason == ErrorReason.DuplicateRecord)
            {
                return new StatusCodeResult(409);
            }
        }
                
        [HttpGet("api/transactions/history/from/{address}")]
        public async Task<IEnumerable<HistoricalTransactionContract>> GetTransactionsFromAddress(string address, int take, string afterHash = null)
        {
            return await _transactionHistoryService.GetTransactionsFromAddress(address, take, afterHash);
        }
        
        [HttpGet("api/transactions/history/to/{address}")]
        public async Task<IEnumerable<HistoricalTransactionContract>> GetTransactionsToAddress(string address, int take, string afterHash = null)
        {
            return await _transactionHistoryService.GetTransactionsToAddress(address, take, afterHash);
        }
        
        [HttpDelete("api/transactions/history/from/{address}/observation")]
        public async Task<IActionResult> UnsubscribeFromAddressHistory(string address)
        {
            try
            {
                await _transactionHistoryService.UnsubscribeAddressFromHistory(address);
                return Ok();
            }
            catch (BusinessException e) when(e.Reason == ErrorReason.RecordNotFound)
            {
                return NoContent();
            }
        }
        
        [HttpDelete("api/transactions/history/to/{address}/observation")]
        public async Task<IActionResult> UnsubscribeToAddressHistory(string address)
        {
            try
            {
                await _transactionHistoryService.UnsubscribeAddressToHistory(address);
                return Ok();
            }
            catch (BusinessException e) when(e.Reason == ErrorReason.RecordNotFound)
            {
                return NoContent();
            }
        }


        #region Not implemented endpoints

        [HttpPost("api/transactions/many-inputs")]
        public IActionResult BuildManyInputsTransaction([FromBody] BuildTransactionWithManyInputsRequest request)
        {
            return StatusCode((int) HttpStatusCode.NotImplemented);
        }
        
        [HttpPost("api/transactions/many-outputs")]
        public IActionResult BuildManyOutputsTransaction([FromBody] BuildTransactionWithManyOutputsRequest request)
        {
            return StatusCode((int) HttpStatusCode.NotImplemented);
        }

        [HttpPut("api/transactions")]
        public IActionResult RebuildTransaction([FromBody] RebuildTransactionRequest request)
        {
            return StatusCode((int) HttpStatusCode.NotImplemented);
        }

        [HttpGet("api/transactions/broadcast/many-inputs/{operationId}")]
        public async Task<IActionResult> GetBroadcastedManyInputsTx(Guid operationId)
        {
            return StatusCode((int) HttpStatusCode.NotImplemented);
        }
        
        [HttpGet("api/transactions/broadcast/many-outputs/{operationId}")]
        public async Task<IActionResult> GetBroadcastedManyOutputsTx(Guid operationId)
        {
            return StatusCode((int) HttpStatusCode.NotImplemented);
        }
        
        #endregion
    }
}
