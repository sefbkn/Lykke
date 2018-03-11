using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        
        /// <summary>
        /// Returns no more than 'take' unspent transaction hashes
        /// for the given address, occuring after 'afterHash'
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetUnspentTransactionIds(string address, int take, string afterHash);

        /// <summary>
        /// Returns all unspent outpoints for this address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<IEnumerable<UnspentTxOutput>> GetUnspentOutputs(string address);
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
        
        public async Task<IEnumerable<string>> GetUnspentTransactionIds(string address, int take, string afterHash)
        {
            var query = 
                @"select funding_tx_hash
                  from addresses addr
                  join transactions tx on tx.tx_hash = addr.funding_tx_hash 
                    and (@after_hash = null or tx.id > (select id from transactions where tx_hash = @after_hash))
                  where address = '@address' and spending_tx_hash is null
                  limit @take";
            
            return await _dbConnection.QueryAsync<string>(query, 
                new { address = address, take = take, afterHash = @afterHash });
        }
        
        public async Task<IEnumerable<UnspentTxOutput>> GetUnspentOutputs(string address)
        {
            const string query = @"select
                    vouts.tx_tree Tree,
                    vouts.tx_hash as Hash,
                    addr.funding_tx_vout_index OutputIndex,
                    vouts.version as OutputVersion,
                    vouts.value as OutputValue,
                    tx.block_height as BlockHeight,
                    tx.block_index as BlockIndex
                from addresses addr
                join vouts on addr.vout_row_id = vouts.id
                join transactions tx on tx.tx_hash = vouts.tx_hash
                where
                    addr.address = @address
                    and addr.spending_tx_hash is null
                    and vouts.script_type = 'pubkeyhash'";

            var result = 
                await _dbConnection.QueryAsync<UnspentTxOutput>(
                    query,
                    new { address = address }
                );

            return result.ToArray();
        }
    }

    public class TxHistoryResult
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string Amount { get; set; }
        public string Hash { get; set; }
    }
    
    public class UnspentTxOutput
    {
        public byte Tree { get; set; }
        public string Hash { get; set; }
        public long OutputVersion { get; set; }
        public uint OutputIndex { get; set; }
        public long OutputValue { get; set; }
        public uint BlockHeight { get; set; }
        public uint BlockIndex { get; set; }
    }
}
