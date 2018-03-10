using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Decred.BlockExplorer
{
    /// <summary>
    /// Base class for http client to reach dcrdata api
    /// </summary>
    public abstract class HttpApiClient
    {
        private readonly HttpClient _client;
        private readonly Uri _apiEndpoint;

        protected HttpApiClient(HttpClient client, Uri apiEndpoint)
        {
            _client = client;
            _apiEndpoint = apiEndpoint ?? throw new ArgumentNullException(nameof(apiEndpoint));
        }

        protected async Task<string> GetResponseAsync(string path)
        {
            var url = _apiEndpoint + path;
            return await _client.GetStringAsync(url);
        }
        
        protected async Task<T> GetResponseAsync<T>(string path)
        {
            var response = await GetResponseAsync(path);
            return JsonConvert.DeserializeObject<T>(response);
        }
    }
}
