using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Decred.BlockExplorer
{
    /// <summary>
    /// Readonly client to read data from the dcrdata postgres database.
    /// </summary>
    public class DcrdataPgClient : IAddressRepository, IBlockRepository
    {
        private readonly IDbConnection _dbConnection;

        public DcrdataPgClient(IDbConnection dbConnection)
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

        public async Task<Block> GetHighestBlock()
        {
            var result = await _dbConnection.QueryAsync<Block>("select max(height) as Height from blocks");
            return result.First();
        }
    }
}
