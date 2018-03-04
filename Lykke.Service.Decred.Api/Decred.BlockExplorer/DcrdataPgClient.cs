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
    public class DcrdataPgClient : IAddressBalanceRepository, IBlockRepository
    {
        private readonly Func<Task<IDbConnection>> _connectionFactory;

        public DcrdataPgClient(Func<Task<IDbConnection>> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<AddressBalance[]> GetAddressBalancesAsync(long blockHeight, string[] addresses)
        {
            // Initialize all balances to zero
            var balances = addresses
                .Select(addr => new AddressBalance { Block = blockHeight, Address = addr, Balance = 0 })
                .ToDictionary(b => b.Address);
            
            using (var db = await _connectionFactory())
            {                
                // Query the database for addresses
                var results = await db.QueryAsync<AddressBalance>(
                   @"select address as Address, sum(value) as Balance from addresses " +
                    "join transactions on transactions.id = funding_tx_row_id " +
                    "where block_height < @blockHeight and address in @addresses and spending_tx_hash is null " +
                    "group by address ",
                    new { blockHeight = blockHeight, addresses = addresses });

                // Set the balances
                foreach (var result in results)
                    balances[result.Address].Balance = result.Balance;
                
                return balances.Select(b => b.Value).ToArray();
            }
        }

        public async Task<long> GetHighestBlock()
        {
            using (var db = await _connectionFactory())
            {
                return await db.ExecuteScalarAsync<int>("select max(height) from blocks");
            }
        }
    }
}
