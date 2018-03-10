using System;
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

        public OperationController(TransactionHistoryService transactionHistoryService)
        {
            _transactionHistoryService = transactionHistoryService;
        }
        
        [HttpGet("api/capabilities")]
        public async Task<CapabilitiesResponse> GetCapabilities()
        {
            throw new NotImplementedException();
        }
        
        [HttpPost("api/transactions/single")]
        public async Task<BuildTransactionResponse> BuildSingleTransaction(
            [FromBody] BuildSingleTransactionRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("api/transactions/many-inputs")]
        public async Task<BuildTransactionResponse> BuildManyInputsTransaction(
            [FromBody] BuildTransactionWithManyInputsRequest request)
        {
            throw new NotImplementedException();
        }
        
        [HttpPost("api/transactions/many-outputs")]
        public async Task<BuildTransactionResponse> BuildManyOutputsTransaction(
            [FromBody] BuildTransactionWithManyOutputsRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("api/transactions")]
        public async Task<RebuildTransactionResponse> RebuildTransaction(
            [FromBody] RebuildTransactionRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("api/transactions/broadcast/single/{operationId}")]
        public async Task<object> GetBroadcastedSingleTx(Guid operationId)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("api/transactions/broadcast/many-inputs/{operationId}")]
        public async Task<object> GetBroadcastedManyInputsTx(Guid operationId)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("api/transactions/broadcast/many-outputs/{operationId}")]
        public async Task<object> GetBroadcastedManyOutputsTx(Guid operationId)
        {
            throw new NotImplementedException();
        }

        [HttpPost("api/transactions/broadcast")]
        public async Task<BroadcastedTransactionOutputContract> Broadcast(
            [FromBody] BroadcastTransactionRequest request)
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
        public async Task<PaginationResponse<HistoricalTransactionContract>> GetTransactionsFromAddress(string address, int take, string afterHash = null)
        {
            return await _transactionHistoryService.GetTransactionsFromAddress(address, take, afterHash);
        }
        
        [HttpGet("api/transactions/history/to/{address}")]
        public async Task<PaginationResponse<HistoricalTransactionContract>> GetTransactionsToAddress(string address, int take, string afterHash = null)
        {
            return await _transactionHistoryService.GetTransactionsToAddress(address, take, afterHash);
        }
    }
}
