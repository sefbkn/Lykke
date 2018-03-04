using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Decred.BlockExplorer
{
    /// <summary>
    /// HTTP client for dcrdata
    /// </summary>
    public class DcrdataHttpClient : BlockExplorer
    {
        private readonly HttpClient _client;
        private readonly Uri _apiEndpoint;

        public DcrdataHttpClient(HttpClient client, Uri apiEndpoint)
        {
            _client = client;
            _apiEndpoint = apiEndpoint ?? throw new ArgumentNullException(nameof(apiEndpoint));
        }

        public override async Task<AddressTxRaw[]> GetAddressTxRawAsync(string address, int? count = 0)
        {
            return await GetResponseAsync<AddressTxRaw[]>($"api/address/{address}/count/{count}/raw");
        }

        private async Task<T> GetResponseAsync<T>(string path)
        {
            var url = _apiEndpoint + path;
            var response = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(response);
        }
    }
}

