using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Decred.BlockExplorer
{
    public interface ITransactionRepository
    {        
        /// <summary>
        /// Retrieves transactions spent by the address in ascending order (oldest first)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="take"></param>
        /// <param name="afterHash"></param>
        /// <returns></returns>
        Task<TxHistoryResult[]> GetTransactionsFromAddress(string address, int take, string afterHash);
        
        /// <summary>
        /// Retrieves transactions spent to the address in ascending order (oldest first)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="take"></param>
        /// <param name="afterHash"></param>
        /// <returns></returns>
        Task<TxHistoryResult[]> GetTransactionsToAddress(string address, int take, string afterHash);
        
        /// <summary>
        /// Returns all unspent outpoints for this address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<UnspentTxOutput[]> GetUnspentTxOutputs(string address);
    }
    
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IDbConnection _dbConnection;

        public TransactionRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        
        public async Task<long?> GetTransactionRowId(string hash)
        {
            return await _dbConnection.ExecuteScalarAsync<long?>(
                "select id from transactions where tx_hash = @txHash",
                new { txHash = hash });
        }

        public async Task<TxHistoryResult[]> GetTransactionsFromAddress(string address, int take, string afterHash)
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
                group by from_addr.address, to_addr.address, to_addr.value, to_addr.funding_tx_hash, to_addr.funding_tx_row_id
                order by to_addr.funding_tx_row_id asc
                limit @take";

            var minTxIdExclusive = await GetTransactionRowId(afterHash) ?? 0;
            var results = await _dbConnection.QueryAsync<TxHistoryResult>(query,
                new { address = address, take = take, minTxId = minTxIdExclusive });
            return results.ToArray();
        }
        
        public async Task<TxHistoryResult[]> GetTransactionsToAddress(string address, int take, string afterHash)
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
            var results = await _dbConnection.QueryAsync<TxHistoryResult>(query,
                new { address = address, take = take, minTxId = minTxIdExclusive });
            return results.ToArray();
        }
        
        public async Task<UnspentTxOutput[]> GetUnspentTxOutputs(string address)
        {
            const string query = @"select
                    vouts.tx_tree Tree,
                    vouts.tx_hash as Hash,
                    addr.funding_tx_vout_index OutputIndex,
                    vouts.version as OutputVersion,
                    vouts.value as OutputValue,
                    tx.block_height as BlockHeight,
                    tx.block_index as BlockIndex,
                    vouts.pkscript as PkScript
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
}
