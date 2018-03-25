using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Decred.BlockExplorer
{
    public interface IAddressRepository
    {
        /// <summary>
        /// Determines the unspent balance of each address at a point in time.
        /// </summary>
        /// <param name="maxBlockHeight"></param>
        /// <param name="addresses"></param>
        /// <returns></returns>
        Task<AddressBalance[]> GetAddressBalancesAsync(long maxBlockHeight, string[] addresses);
    }

    /// <summary>
    /// Readonly client to read data from the dcrdata postgres database.
    /// </summary>
    public class AddressRepository : IAddressRepository
    {
        private readonly IDbConnection _dbConnection;

        public AddressRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<AddressBalance[]> GetAddressBalancesAsync(long maxBlockHeight, string[] addresses)
        {
            const string query = 
                @"select address as Address, sum(value) as Balance from addresses " +
                 "join transactions on transactions.id = funding_tx_row_id " +
                 "where block_height <= @maxBlockHeight and address = any(@addresses) and spending_tx_hash is null " +
                 "group by address";
            
            var results = (await _dbConnection.QueryAsync<AddressBalance>(query, 
                new { maxBlockHeight = maxBlockHeight, addresses = addresses })).ToList();

            // Since some addresses with 0 balance may not be returned, make sure return value has
            // corresponding value for each provided address.
            var balances = addresses.Select(address => new AddressBalance
            {
                Address = address,
                Block = maxBlockHeight
            }).ToDictionary(balance => balance.Address);

            foreach (var result in results)
                balances[result.Address].Balance = result.Balance;
            
            return balances.Values.ToArray();
        }

        /// <summary>
        /// Returns utxo information for a given address.
        /// 
        /// All fields should be enough to support creating p2pkh unlocking constructs
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UnspentTransactionOutput>> GetUnspentTransactionOutputs(string address)
        {
            var query = @"select
                    vouts.tx_hash as TxHash,
                    vouts.tx_tree TxTree,
                    vouts.tx_index TxIndex,
                    vouts.version as TxVersion,
                    vouts.pkscript as PkScript,
                    vouts.value as OutputValue,
                    addr.funding_tx_vout_index TxOutIndex
                from addresses addr
                join vouts on addr.vout_row_id = vouts.id
                where address = @address
                    and addr.spending_tx_hash is null
                    and vouts.script_type = 'pubkeyhash'";
            
            return await _dbConnection.QueryAsync<UnspentTransactionOutput>(query,
                new{ address = address });
        }
    }

    public class UnspentTransactionOutput
    {
        public string TxHash { get; set; }
        public byte TxTree { get; set; }
        public int TxIndex { get; set; }
        public ushort TxVersion { get; set; }
        public byte[] PkScript { get; set; }
    }
}
