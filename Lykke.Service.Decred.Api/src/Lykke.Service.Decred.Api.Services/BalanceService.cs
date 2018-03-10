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
        private readonly IObservableOperationRepository<ObservableWalletEntity> _observableWalletRepository;
        private readonly IAddressRepository _balanceRepository;
        private readonly IBlockRepository _blockRepository;
        private readonly IAddressValidationService _addressValidator;

        public BalanceService(
            IObservableOperationRepository<ObservableWalletEntity> observableWalletRepository,
            IAddressRepository balanceRepository,
            IBlockRepository blockRepository,
            IAddressValidationService addressValidator)
        {
            _observableWalletRepository = observableWalletRepository;
            _balanceRepository = balanceRepository;
            _blockRepository = blockRepository;
            _addressValidator = addressValidator;
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
            if (!_addressValidator.IsValid(address))
                throw new BusinessException(ErrorReason.InvalidAddress);
            
            await _observableWalletRepository.InsertAsync(new ObservableWalletEntity { Address = address });
        }

        /// <summary>
        /// Removes given address from observation repository.
        /// If the address is not in the repository, a BusinessException is thrown.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task UnsubscribeAsync(string address)
        {            
            var entity = new ObservableWalletEntity(){ Address = address };
            await _observableWalletRepository.DeleteAsync(entity);
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

            var addressBalances = await Task.WhenAll(
                from address in addresses
                select _balanceRepository.GetAddressBalanceAsync(block.Height, address)
            );
            
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
