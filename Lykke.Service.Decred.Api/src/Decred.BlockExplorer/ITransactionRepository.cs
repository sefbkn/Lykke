using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using Dapper;

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
        
        /// <summary>
        /// Retrieves transactions spent by the address in ascending order (oldest first)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="take"></param>
        /// <param name="afterHash"></param>
        /// <returns></returns>
        Task<IEnumerable<TxHistoryResult>> GetTransactionsFromAddress(string address, int take, string afterHash);
        
        /// <summary>
        /// Retrieves transactions spent to the address in ascending order (oldest first)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="take"></param>
        /// <param name="afterHash"></param>
        /// <returns></returns>
        Task<IEnumerable<TxHistoryResult>> GetTransactionsToAddress(string address, int take, string afterHash);
    }
    
    public class TransactionRepository : HttpApiClient, ITransactionRepository
    {
        private readonly IDbConnection _dbConnection;

        public TransactionRepository(IDbConnection dbConnection, HttpClient client, Uri apiEndpoint) : base(client, apiEndpoint)
        {
            _dbConnection = dbConnection;
        }
        
        public async Task<string> GetRawTransactionById(string txid)
        {
            return await GetResponseAsync($"api/tx/hex/{txid}");
        }

        public async Task<long?> GetTransactionRowId(string hash)
        {
            return await _dbConnection.ExecuteScalarAsync<long?>(
                "select id from transactions where tx_hash = @txHash",
                new { txHash = hash });
        }

        public async Task<IEnumerable<TxHistoryResult>> GetTransactionsFromAddress(string address, int take, string afterHash)
        {            
            const string query = 
                @"select
                    from_addr.address as FromAddress,
                    to_addr.address as ToAddress,
                    to_addr.value as Amount,
                    to_addr.funding_tx_hash as Hash
                from addresses from_addr
                join addresses to_addr on to_addr.funding_tx_hash = from_addr.spending_tx_hash
                where from_addr.address = @address and to_addr.funding_tx_row_id > @minTxId
                group by to_addr.address, to_addr.value, to_addr.funding_tx_hash, to_addr.funding_tx_row_id
                order by to_addr.funding_tx_row_id asc
                limit @take";

            var minTxIdExclusive = await GetTransactionRowId(afterHash) ?? 0;
            return await _dbConnection.QueryAsync<TxHistoryResult>(query,
                new { address = address, take = take, minTxId = minTxIdExclusive });
        }
        
        public async Task<IEnumerable<TxHistoryResult>> GetTransactionsToAddress(string address, int take, string afterHash)
        {            
            const string query = 
                @"select
                    from_addr.address as FromAddress,
                    to_addr.address as ToAddress,
                    to_addr.value as Amount,
                    to_addr.funding_tx_hash as Hash
                from addresses from_addr
                join addresses to_addr on to_addr.funding_tx_hash = from_addr.spending_tx_hash
                where to_addr.address = @address and to_addr.funding_tx_row_id > @minTxId
                group by from_addr.address, to_addr.address, to_addr.value, to_addr.funding_tx_hash, to_addr.funding_tx_row_id
                order by to_addr.funding_tx_row_id asc
                limit @take";

            var minTxIdExclusive = await GetTransactionRowId(afterHash) ?? 0;
            return await _dbConnection.QueryAsync<TxHistoryResult>(query,
                new { address = address, take = take, minTxId = minTxIdExclusive });
        }
    }

    public class TxHistoryResult
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string Amount { get; set; }
        public string Hash { get; set; }
    }
}
