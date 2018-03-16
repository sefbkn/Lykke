using System;
using System.Linq;
using System.Threading.Tasks;
using Decred.BlockExplorer;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Decred.Api.Repository;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Decred.Api.Services
{
    public enum TxDirection
    {
        Incoming = 0,
        Outgoing = 1
    }

    public class ObservableAddressActivityEntity : TableEntity
    {
        private string _address;

        public ObservableAddressActivityEntity()
        {
            PartitionKey = "ByDirectedAddress";
        }
        
        public ObservableAddressActivityEntity(string address, TxDirection direction) : this()
        {
            Address = address;
            TxDirection = direction;
        }

        public string Address
        {
            get { return _address; }
            set { _address = value;
                RowKey = value;
            }
        }

        public TxDirection TxDirection { get; set; }
        public string DirectedAddress {
            get { return Address + TxDirection; }
            set {  }
        }
    }
    
    public class TransactionHistoryService
    {
        private readonly ITransactionRepository _txRepo;
        private readonly IObservableOperationRepository<ObservableAddressActivityEntity> _operationRepo;

        public TransactionHistoryService(
            ITransactionRepository txRepo,
            IObservableOperationRepository<ObservableAddressActivityEntity> operationRepo)
        {
            _txRepo = txRepo;
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
            var entity = new ObservableAddressActivityEntity(address, TxDirection.Outgoing);
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
            var entity = new ObservableAddressActivityEntity(address, TxDirection.Incoming);
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
            var entity = new ObservableAddressActivityEntity(address, TxDirection.Outgoing);
            await _operationRepo.DeleteAsync(entity);
        }
        
        /// <summary>
        /// Stop observing spending transactions
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task UnsubscribeAddressToHistory(string address)
        {
            var entity = new ObservableAddressActivityEntity(address, TxDirection.Incoming);
            await _operationRepo.DeleteAsync(entity);
        }
        
        /// <summary>
        /// Finds known transactions for a particular address occuring after the transaction with the given hash
        /// 
        /// If afterHash is null, the earliest known transaction for the given address is returned first.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="take"></param>
        /// <param name="afterHash"></param>
        /// <returns></returns>
        public async Task<HistoricalTransactionContract[]> GetTransactionsFromAddress(string address, int take, string afterHash)
        {
            var results = await _txRepo.GetTransactionsFromAddress(address, take, afterHash);
            return results.Select(r => new HistoricalTransactionContract
            {
                Amount = r.Amount,
                AssetId = "DCR",
                FromAddress = r.FromAddress,
                ToAddress = r.ToAddress,
                Hash = r.Hash,
                
                // TODO: Fill these values in
                
                // If the transaction was broadcast using the Lykke decred service, log the operation id.
                OperationId = Guid.Empty,
                // Timestamp that the transaction occurred.
                Timestamp = DateTime.MinValue
            }).ToArray();
            // Match up transactions that have an operation id...
        }
        
        public async Task<HistoricalTransactionContract[]> GetTransactionsToAddress(string address, int take, string afterHash = null)
        {
            var results = await _txRepo.GetTransactionsFromAddress(address, take, afterHash);
            
            return results.Select(r => new HistoricalTransactionContract
            {
                Amount = r.Amount,
                AssetId = "DCR",
                FromAddress = r.FromAddress,
                ToAddress = r.ToAddress,
                Hash = r.Hash,
                
                // TODO: Fill these values in 
                
                // If the transaction was broadcast using the Lykke decred service, log the operation id.
                OperationId = Guid.Empty,
                // Timestamp that the transaction occurred.
                Timestamp = DateTime.MinValue
            }).ToArray();
        }
    }
}
