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
        Task<AddressBalance[]> GetAddressBalancesAsync(string[] addresses, long blockHeight);
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

        public async Task<AddressBalance[]> GetAddressBalancesAsync(string[] addresses, long blockHeight)
        {
            const string query = 
                @"select address as Address, sum(value) as Balance from addresses " +
                 "join transactions on transactions.id = funding_tx_row_id " +
                 "where block_height <= @blockHeight and address = any(@addresses) and spending_tx_hash is null " +
                 "group by address";
            
            var results = (await _dbConnection.QueryAsync<AddressBalance>(query, 
                new { blockHeight = blockHeight, addresses = addresses })).ToList();
            

            // Since some addresses with 0 balance may not be returned, make sure return value has
            // corresponding value for each provided address.
            var balances = addresses.Select(address => new AddressBalance
            {
                Address = address,
                Block = blockHeight
            }).ToDictionary(balance => balance.Address);

            foreach (var result in results)
                balances[result.Address].Balance = result.Balance;
            
            return balances.Values.ToArray();
        }
    }
}
