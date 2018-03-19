using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Decred.Common.Client
{
    public class DcrdHttpClient : IDcrdClient
    {
        private readonly string _apiUrl;
        private readonly HttpClientHandler _httpClientHandler;

        public DcrdHttpClient(string apiUrl, HttpClientHandler httpClientHandler)
        {
            _apiUrl = apiUrl;
            _httpClientHandler = httpClientHandler;
        }

        private async Task<DcrdRpcResponse<T>> Perform<T>(string method, params object[] parameters)
        {
            using (var httpClient = new HttpClient(_httpClientHandler, false))
            {
                var request = new
                {
                    jsonrpc = "1.0",
                    id = "0",
                    method = method,
                    @params = parameters
                };
                
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(_apiUrl, content);

                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DcrdRpcResponse<T>>(responseString);
            }
        }

        public async Task<DcrdRpcResponse<string>> PingAsync()
        {
            return await Perform<string>("ping");
        }

        public async Task<DcrdRpcResponse<string>> SendRawTransactionAsync(string hexTransaction)
        {
            return await Perform<string>("sendrawtransaction", hexTransaction);
        }

        public async Task<GetBestBlockResult> GetBestBlockAsync()
        {
            var result = await Perform<GetBestBlockResult>("getbestblock");
            return result.Result;
        }

        public async Task<decimal> EstimateFeeAsync(int numBlocks)
        {
            var result = await Perform<decimal>("estimatefee", numBlocks);
            return result.Result;
        }
    }
}
