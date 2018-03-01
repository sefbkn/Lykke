using System;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract.Common;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Decred.Api.Controllers
{
    public class OperationController : Controller
    {
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
            throw new NotImplementedException();
        }
        
        [HttpPost("api/transactions/history/to/{address}/observation")]
        public async Task<IActionResult> SubscribeAddressTo(string address)
        {
            throw new NotImplementedException();
        }

        [HttpGet("api/transactions/history/from/{address}")]
        public async Task<IActionResult> GetFromAddressHistory(string address, int take, string afterHash = null)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("/api/transactions/history/to/{address}")]
        public async Task<IActionResult> GetToAddressHistory(string address, int take, string afterHash = null)
        {
            throw new NotImplementedException();
        }
        
        [HttpDelete("api/transactions/history/from/{address}/observation")]
        public async Task<IActionResult> UnsubscribeFromAddressHistory(string address, int take, string afterHash = null)
        {
            // Should stop observation of the transactions that transfer fund from the address
            throw new NotImplementedException();
        }
        
        [HttpDelete("api/transactions/history/to/{address}/observation")]
        public async Task<IActionResult> UnsubscribeToAddressHistory(string address, int take, string afterHash = null)
        {
            // Should stop observation of the transactions that transfer fund to the address
            throw new NotImplementedException();
        }
    }
}
