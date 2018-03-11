﻿using System;
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

        /// <summary>
        /// Discovers the balance of the given addresses at a particular block height
        /// </summary>
        /// <param name="blockHeight"></param>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public async Task<AddressBalance> GetAddressBalanceAsync(long blockHeight, string address)
        {
            var balance = new AddressBalance { Address = address, Block = blockHeight };
            
            var query = 
                @"select address as Address, sum(value) as Balance from addresses " +
                "join transactions on transactions.id = funding_tx_row_id " +
                "where block_height <= @blockHeight and address = @address and spending_tx_hash is null " +
                "group by address";
            
            var results = (await _dbConnection.QueryAsync<AddressBalance>(query, 
                new { blockHeight = blockHeight, address = address })).ToList();

            if (results.Any())
            {
                balance.Balance = results.First().Balance;
            }

            return balance;
        }

        /// <summary>
        /// Finds all unspent transaction ids for a particular address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="take"></param>
        /// <param name="afterHash"></param>
        /// <returns></returns>
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

        public async Task<Block> GetHighestBlock()
        {
            var result = await _dbConnection.QueryAsync<Block>("select max(height) as Height from blocks");
            return result.First();
        }
    }
}