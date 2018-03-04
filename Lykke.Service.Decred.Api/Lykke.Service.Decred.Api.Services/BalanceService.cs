using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Decred.BlockExplorer;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.Decred.Api.Common;
using Lykke.Service.Decred.Api.Repository;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Paymetheus.Decred.Wallet;

namespace Lykke.Service.Decred.Api.Services
{    
    public class BalanceService
    {
        private const int RecordNotFoundStatus = 404;
        private const int DuplicateRecordStatus = 409;
        
        private readonly IObservableOperationRepository<ObservableWalletEntity> _observableWalletRepository;
        private readonly IAddressBalanceRepository _balanceRepository;
        private readonly IBlockRepository _blockRepository;

        public BalanceService(
            IObservableOperationRepository<ObservableWalletEntity> observableWalletRepository,
            IAddressBalanceRepository balanceRepository,
            IBlockRepository blockRepository)
        {
            _observableWalletRepository = observableWalletRepository;
            _balanceRepository = balanceRepository;
            _blockRepository = blockRepository;
        }
        
        /// <summary>
        /// Adds the given address to the observation repository.
        /// 
        /// If the address is already in the repository, a BusinessException is thrown.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(string address)
        {
            try
            {
                var entity = new ObservableWalletEntity { Address = address };
                await _observableWalletRepository.InsertAsync(entity);
            }
            catch (StorageException e) when(e.RequestInformation.HttpStatusCode == DuplicateRecordStatus)
            {
                throw new BusinessException("Already observing balance for address", e);
            }
        }

        /// <summary>
        /// Removes given address from observation repository.
        /// 
        /// If the address is not in the repository, a BusinessException is thrown.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task UnsubscribeAsync(string address)
        {
            try
            {
                await _observableWalletRepository.DeleteAsync(new ObservableWalletEntity { Address = address });
            }
            catch (StorageException ex) when(ex.RequestInformation.HttpStatusCode == RecordNotFoundStatus)
            {
                throw new BusinessException("Address is not being observed", ex);
            }
        }

        /// <summary>
        /// Retrieves balances for all subscribed addresses
        /// </summary>
        /// <param name="take">Max number of balances to retrieve</param>
        /// <param name="continuation">Determines position in the data set to start from</param>
        /// <returns></returns>
        public async Task<PaginationResponse<WalletBalanceContract>> GetBalancesAsync(int take, string continuation)
        {
            var result = await _observableWalletRepository.GetDataWithContinuationTokenAsync(take, continuation);
            
            var addresses = result.Entities.Select(e => e.Address).ToArray();
            var block = await _blockRepository.GetHighestBlock();
            var addressBalances = await _balanceRepository.GetAddressBalancesAsync(block.Height, addresses);
            
            var balances = addressBalances.Select(b => new WalletBalanceContract {
                AssetId = "DCR",
                Block = b.Block,
                Address = b.Address,
                Balance = b.Balance.ToString(),
            }).ToArray();
            
            return new PaginationResponse<WalletBalanceContract> { 
                Items = balances, 
                Continuation = result.ContinuationToken
            };
        }
    }
}
