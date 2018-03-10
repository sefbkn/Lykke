using System;
using System.Threading.Tasks;
using Decred.BlockExplorer;
using Lykke.Service.BlockchainApi.Contract;
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
        public ObservableAddressActivityEntity()
        {
            PartitionKey = "ByDirectedAddress";
        }
        
        public ObservableAddressActivityEntity(string address, TxDirection direction) : this()
        {
            Address = address;
            TxDirection = direction;
        }
        
        public string Address { get; set; }
        public TxDirection TxDirection { get; set; }
        public string DirectedAddress => Address + TxDirection;
    }
    
    public class TransactionHistoryService
    {
        private readonly IBlockRepository _blockRepository;
        private readonly IObservableOperationRepository<ObservableAddressActivityEntity> _operationRepo;

        public TransactionHistoryService(
            IBlockRepository blockRepository,
            IObservableOperationRepository<ObservableAddressActivityEntity> operationRepo)
        {
            _blockRepository = blockRepository;
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
        
        public async Task<PaginationResponse<HistoricalTransactionContract>> GetTransactionsFromAddress(string address, int take, string afterHash = null)
        {
            throw new NotImplementedException();
        }
        
        public async Task<PaginationResponse<HistoricalTransactionContract>> GetTransactionsToAddress(string address, int take, string afterHash = null)
        {
            throw new NotImplementedException();
        }
    }
}
