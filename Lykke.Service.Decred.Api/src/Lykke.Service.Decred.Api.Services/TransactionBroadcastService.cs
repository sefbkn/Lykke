using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Decred.BlockExplorer;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using NDecred.Common;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Encoders;

namespace Lykke.Service.Decred.Api.Services
{
    public interface ITransactionBroadcastService
    {
        Task<BroadcastedSingleTransactionResponse> Broadcast(Guid operationId, string hexTransaction);
    }
    
    public class TransactionBroadcastService : ITransactionBroadcastService
    {
        private readonly DcrdConfig _dcrdConfig;
        private readonly IBlockRepository _blockRepository;

        public TransactionBroadcastService(DcrdConfig dcrdConfig, IBlockRepository blockRepository)
        {
            _dcrdConfig = dcrdConfig;
            _blockRepository = blockRepository;
        }
        
        public async Task<BroadcastedSingleTransactionResponse> Broadcast(Guid operationId, string hexTransaction)
        {
            var httpClient = new DcrdHttpClient(_dcrdConfig.DcrdApiUrl, _dcrdConfig.HttpClientHandler);
            var result = await httpClient.BroadcastTransactionAsync(hexTransaction);                
            if (result.Error != null)
                throw new TransactionBroadcastException($"[{result.Error.Code}] {result.Error.Message}");
            
            var block = await _blockRepository.GetHighestBlock();
            var transaction = new MsgTx();
            transaction.Decode(HexUtil.ToByteArray(hexTransaction));

            var txHash = HexUtil.FromByteArray(transaction.GetHash().Reverse().ToArray());

            // Sum the values that are not change
            var amount = transaction.TxOut.Sum(o => o.Value);
            var fee = transaction.TxIn.Sum(i => i.ValueIn) - amount;

            return new BroadcastedSingleTransactionResponse
            {
                Amount = amount.ToString(),
                Fee = fee.ToString(),
                Block = block.Height,
                Error = "",
                ErrorCode = null,
                Hash = txHash,
                OperationId = operationId,
                State = BroadcastedTransactionState.InProgress,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public class TransactionBroadcastException : Exception
    {
        public TransactionBroadcastException(string message = null, Exception innerException = null) 
            : base(message, innerException)
        {
        }
    }

    public class DcrdConfig
    {
        public string DcrdApiUrl { get; set; }
        public HttpClientHandler HttpClientHandler { get; set; }
    }
    
    public class DcrdHttpClient
    {
        private readonly string _apiUrl;
        private readonly HttpClientHandler _httpClientHandler;

        public DcrdHttpClient(string apiUrl, HttpClientHandler httpClientHandler)
        {
            _apiUrl = apiUrl;
            _httpClientHandler = httpClientHandler;
        }

        public async Task<JsonRpcResponse> BroadcastTransactionAsync(string hexTransaction)
        {
            using (var httpClient = new HttpClient(_httpClientHandler))
            {
                var request = new
                {
                    jsonrpc = "1.0",
                    id = "0",
                    method = "sendrawtransaction",
                    @params = new[]{ hexTransaction }
                };
                
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(_apiUrl, content);

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<JsonRpcResponse>(responseString);
                return responseObject;
            }
        }
        
        public class JsonRpcResponse
        {
            public string Id { get; set; }
            public string Jsonrpc { get; set; }
            public string Result { get; set; }
            public RpcError Error { get; set; }
            
            public class RpcError
            {
                public int? Code { get; set; }
                public string Message { get; set; }
            }
        }
    }
}
