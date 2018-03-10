using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Decred.BlockExplorer
{
    /// <summary>
    /// HTTP client for dcrdata
    /// </summary>
    public class DcrdataHttpClient : HttpApiClient
    {
        public DcrdataHttpClient(HttpClient client, Uri apiEndpoint) : base(client, apiEndpoint)
        {
        }

        public async Task<AddressTxRaw[]> GetAddressTxRawAsync(string address, int? count = 0)
        {
            return await GetResponseAsync<AddressTxRaw[]>($"api/address/{address}/count/{count}/raw");
        }
    }
}

