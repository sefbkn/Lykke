using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Decred.BlockExplorer;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Decred.Api.Common;
using Lykke.Service.Decred.Api.Common.Entity;
using Lykke.Service.Decred.Api.Repository;
using NDecred.Common;
using Newtonsoft.Json;

namespace Lykke.Service.Decred.Api.Services
{
    public class TransactionHistoryService
    {
        private readonly ITransactionRepository _txRepo;
        private readonly INosqlRepo<BroadcastedTransactionByHash> _broadcastTxHashRepo;
        private readonly INosqlRepo<ObservableAddressEntity> _operationRepo;

        public TransactionHistoryService(
            ITransactionRepository txRepo,
            INosqlRepo<BroadcastedTransactionByHash> broadcastTxHashRepo,
            INosqlRepo<ObservableAddressEntity> operationRepo)
        {
            _txRepo = txRepo;
            _broadcastTxHashRepo = broadcastTxHashRepo;
            _operationRepo = operationRepo;
        }
        
        /// <summary>
        /// Observe receiving transactions for this address.
        /// Only need this to return expected errors.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task SubscribeAddressFrom(string address)
        {
            var entity = new ObservableAddressEntity(address, TxDirection.Outgoing);
            await _operationRepo.InsertAsync(entity);
        }

        /// <summary>
        /// Observe spending transactions for this address.
        /// Only need this to return expected errors.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task SubscribeAddressTo(string address)
        {
            var entity = new ObservableAddressEntity(address, TxDirection.Incoming);
            await _operationRepo.InsertAsync(entity);
        }
        
        /// <summary>
        /// Stop watching for receiving transactions
        /// </summary>
        /// <param name="address"></param>
        /// <param name="take"></param>
        /// <param name="afterHash"></param>
        /// <returns></returns>
        public async Task UnsubscribeAddressFromHistory(string address)
        {
            var entity = new ObservableAddressEntity(address, TxDirection.Outgoing);
            await _operationRepo.DeleteAsync(entity);
        }
        
        /// <summary>
        /// Stop observing spending transactions
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task UnsubscribeAddressToHistory(string address)
        {
            var entity = new ObservableAddressEntity(address, TxDirection.Incoming);
            await _operationRepo.DeleteAsync(entity);
        }

        /// <summary>
        /// Finds known transactions from a particular address occuring after the transaction with the given hash
        /// 
        /// If afterHash is null, the earliest known transaction for the given address is returned first.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="take"></param>
        /// <param name="afterHash"></param>
        /// <returns></returns>
        public async Task<HistoricalTransactionContract[]> GetTransactionsFromAddress(string address, int take, string afterHash)
        {
            var transactions = await _txRepo.GetTransactionsFromAddress(address, take, afterHash);
            return await GetHistoricalTransactionContracts(transactions);
        }

        public async Task<HistoricalTransactionContract[]> GetTransactionsToAddress(string address, int take, string afterHash = null)
        {
            var transactions = await _txRepo.GetTransactionsToAddress(address, take, afterHash);
            return await GetHistoricalTransactionContracts(transactions);
        }
        
        private async Task<HistoricalTransactionContract[]> GetHistoricalTransactionContracts(TxHistoryResult[] transactions)
        {
            var operationIdLookup = await BuildOperationIdLookup(transactions);
            return transactions.Select(tx => new HistoricalTransactionContract
            {
                Amount = tx.Amount,
                AssetId = "DCR",
                FromAddress = tx.FromAddress,
                ToAddress = tx.ToAddress,
                Hash = tx.Hash,
                Timestamp = DateTimeUtil.FromUnixTime(tx.BlockTime),
                // Match up transactions that have an operation id
                // from a previously broadcast transaction.
                OperationId = operationIdLookup.ContainsKey(tx.Hash) ? operationIdLookup[tx.Hash] : Guid.Empty,
            }).ToArray();
        }

        /// <summary>
        /// Creates a dictionary of transaction hashes to lykke operation ids.
        /// 
        /// Searches for transaction hashes that were broadcast, and retrieves the respective hash value.
        /// </summary>
        /// <param name="transactions"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, Guid>> BuildOperationIdLookup(IEnumerable<TxHistoryResult> transactions)
        {
            // Lookup hashes for operation id
            var txHashes = transactions.Select(r => r.Hash);

            // Check to see if the hash has been broadcast.  Hash is the row key.
            var broadcastedTxHashes = await _broadcastTxHashRepo.GetAsync(txHashes);            
            return broadcastedTxHashes.ToDictionary(p => p.Hash, p => p.OperationId);
        }
    }
}
