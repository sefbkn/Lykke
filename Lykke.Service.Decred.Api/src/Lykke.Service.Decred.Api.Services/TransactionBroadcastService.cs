using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lykke.Service.Decred.Api.Services
{
    public interface ITransactionBroadcastService
    {
        Task Broadcast(string hexTransaction);
    }
    
    public class TransactionBroadcastService : ITransactionBroadcastService
    {
        private readonly DcrdConfig _dcrdConfig;

        public TransactionBroadcastService(DcrdConfig dcrdConfig)
        {
            _dcrdConfig = dcrdConfig;
        }
        
        public async Task Broadcast(string hexTransaction)
        {            
            var httpClient = new DcrdHttpClient(_dcrdConfig.DcrdApiUrl, _dcrdConfig.HttpClientHandler);
            var result = await httpClient.BroadcastTransactionAsync(hexTransaction);
            
            if (result.Error != null)
                throw new TransactionBroadcastException($"[{result.Error.Code}] {result.Error.Message}");
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
