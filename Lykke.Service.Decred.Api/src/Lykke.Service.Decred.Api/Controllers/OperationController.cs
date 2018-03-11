using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Common;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Decred.Api.Common;
using Lykke.Service.Decred.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Decred.Api.Controllers
{
    public class OperationController : Controller
    {
        private readonly TransactionHistoryService _transactionHistoryService;
        private readonly TransactionBuilderService _txBuilderService;

        public OperationController(
            TransactionHistoryService transactionHistoryService,
            TransactionBuilderService txBuilderService)
        {
            _transactionHistoryService = transactionHistoryService;
            _txBuilderService = txBuilderService;
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
                var txResponse = await _txBuilderService.BuildSingleTransactionAsync(request, feeFactor);
                return Json(txResponse);
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

        [HttpGet("api/transactions/broadcast/single/{operationId}")]
        public async Task<object> GetBroadcastedSingleTx(Guid operationId)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("api/transactions/broadcast/many-inputs/{operationId}")]
        public async Task<object> GetBroadcastedManyInputsTx(Guid operationId)
        {
            return StatusCode((int) HttpStatusCode.NotImplemented);
        }
        
        [HttpGet("api/transactions/broadcast/many-outputs/{operationId}")]
        public async Task<object> GetBroadcastedManyOutputsTx(Guid operationId)
        {
            return StatusCode((int) HttpStatusCode.NotImplemented);
        }

        [HttpPost("api/transactions/broadcast")]
        public async Task<BroadcastedTransactionOutputContract> Broadcast([FromBody] BroadcastTransactionRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("api/transactions/broadcast/{operationId}")]
        public async Task<IActionResult> GetObservableOperation(Guid operationId)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("api/transactions/broadcast/{operationId}")]
        public async Task<IActionResult> RemoveObservableOperation(Guid operationId)
        {
            throw new NotImplementedException();
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
                // TODO: Return expected error response.
                Console.WriteLine(e);
                throw;
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
                Console.WriteLine(e);
                throw;
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
    }
}
