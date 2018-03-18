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

        public async Task<DcrdRpcResponse> SendRawTransactionAsync(string hexTransaction)
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
                return JsonConvert.DeserializeObject<DcrdRpcResponse>(responseString);
            }
        }
    }
}
