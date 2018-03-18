using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
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
    public class TransactionController : Controller
    {
        private readonly ILog _log;
        private readonly IUnsignedTransactionService _txBuilderService;
        private readonly ITransactionBroadcastService _txBroadcastService;

        public TransactionController(
            ILog log,
            IUnsignedTransactionService txBuilderService,
            ITransactionBroadcastService txBroadcastService)
        {
            _log = log;
            _txBuilderService = txBuilderService;
            _txBroadcastService = txBroadcastService;
        }
        
        [HttpGet("api/capabilities")]
        public CapabilitiesResponse GetCapabilities()
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
                var response = await _txBuilderService.BuildSingleTransactionAsync(request, feeFactor);
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
        
        [HttpPost("api/transactions/broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastTransactionRequest request)
        {
            try
            {
                // Broadcast the signed transaction
                await _txBroadcastService.Broadcast(request.OperationId, request.SignedTransaction);
                Response.StatusCode = (int) HttpStatusCode.OK;
            }
            catch (BusinessException e) when (e.Reason == ErrorReason.DuplicateRecord)
            {
                // If this operationid was already broadcast, return 409 conflict.
                Response.StatusCode = (int) HttpStatusCode.Conflict;
            }
            catch (TransactionBroadcastException e)
            {
                // Log actual error message from broadcast service
                await _log.WriteErrorAsync(nameof(TransactionController), nameof(Broadcast), request.OperationId.ToString(), e);
                Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            }

            // Neither of the errors:
            //     * amountIsTooSmall
            //     * notEnoughBalance
            // would occur here, since we don't
            // construct or sign transactions with those errors.
            return Json(new {
                errorMessage = ""
            });
        }
        
        [HttpGet("api/transactions/broadcast/single/{operationId}")]
        public async Task<IActionResult> GetBroadcastedSingleTx(Guid operationId)
        {
            try
            {
                // Retrieves a broadcasted transaction
                var result = await _txBroadcastService.GetBroadcastedTxSingle(operationId);
                return Json(result);

            }
            catch (BusinessException e) when (e.Reason == ErrorReason.RecordNotFound)
            {
                return NoContent();
            }
        }


        [HttpDelete("api/transactions/broadcast/{operationId}")]
        public async Task<IActionResult> RemoveObservableOperation(Guid operationId)
        {            
            try
            {
                await _txBroadcastService.UnsubscribeBroadcastedTx(operationId);
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
        public IActionResult GetBroadcastedManyInputsTx(Guid operationId)
        {
            return StatusCode((int) HttpStatusCode.NotImplemented);
        }
        
        [HttpGet("api/transactions/broadcast/many-outputs/{operationId}")]
        public IActionResult GetBroadcastedManyOutputsTx(Guid operationId)
        {
            return StatusCode((int) HttpStatusCode.NotImplemented);
        }
        
        #endregion
    }
}
