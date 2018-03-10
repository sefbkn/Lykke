using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Decred.BlockExplorer
{
    public interface ITransactionRepository
    {
        /// <summary>
        /// Retrieves a hex-encoded transaction by id
        /// </summary>
        /// <param name="transactionId">hash of the transaction</param>
        /// <returns></returns>
        Task<string> GetRawTransactionById(string transactionId);
    }

    public class TransactionHistoryRepository : HttpApiClient, ITransactionRepository
    {
        public TransactionHistoryRepository(HttpClient client, Uri apiEndpoint) : base(client, apiEndpoint)
        {
        }
        
        public async Task<string> GetRawTransactionById(string txid)
        {
            return await GetResponseAsync($"api/tx/hex/{txid}");
        }
    }
}
