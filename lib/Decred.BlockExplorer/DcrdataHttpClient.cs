using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Decred.BlockExplorer
{    
    public interface IDcrdataHttpClient
    {
        /// <summary>
        /// Returns a list of unconfirmed transactions to the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<AddressHistoryResponse> GetTopTransactionsByAddress(string address);
        
        /// <summary>
        /// Returns a hex-encoded raw transaction by hash.
        /// </summary>
        /// <param name="transactionHash"></param>
        /// <returns></returns>
        Task<string> GetRawMempoolTransaction(string transactionHash);
    }
    
    /// <summary>
    /// Http client to communicate with dcrd.
    /// </summary>
    public class DcrdataHttpClient : IDcrdataHttpClient
    {
        private readonly string _apiUrlBase;
        private readonly HttpClientHandler _httpClientHandler;

        public DcrdataHttpClient(string apiUrlBase, HttpClientHandler httpClientHandler)
        {
            _apiUrlBase = apiUrlBase;
            _httpClientHandler = httpClientHandler;
        }

        public async Task<string> GetRawMempoolTransaction(string transactionHash)
        {
            using (var httpClient = new HttpClient(_httpClientHandler, false))
            {
                var url = Path.Combine(_apiUrlBase, $"api/tx/hex/{transactionHash}");
                var response = await httpClient.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<AddressHistoryResponse> GetTopTransactionsByAddress(string address)
        {
            using (var httpClient = new HttpClient(_httpClientHandler, false))
            {
                var url = Path.Combine(_apiUrlBase, $"api/address/{address}");
                var response = await httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AddressHistoryResponse>(json);
            }
        }
    }
    
    public class AddressHistoryResponse
    {
        public string Address { get; set; }
        public AddressTransactions[] AddressTransactions { get; set; }
    }
    
    public class AddressTransactions
    {
        public string TxId { get; set; }
        public long Size { get; set; }
        public long Time { get; set; }
        public decimal Value { get; set; }
        public long Confirmations { get; set; }
    }

}
