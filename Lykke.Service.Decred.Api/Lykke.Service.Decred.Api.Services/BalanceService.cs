using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Decred.BlockExplorer;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Microsoft.WindowsAzure.Storage.Table;
using Paymetheus.Decred.Wallet;

namespace Lykke.Service.Decred.Api.Services
{
    public interface IObservableWalletEntity
    {
        string Address { get; set; }
    }
    
    public class ObservableWalletEntity : TableEntity, IObservableWalletEntity
    {
        public string Address { get; set; }
    }
    
    public class BalanceService
    {
        private readonly BlockExplorer _blockExplorer;
        private readonly IObservableRepository<string, ObservableWalletEntity> _observableWalletRepository;
        private List<string> _subscription = new List<string>();

        public BalanceService(
            BlockExplorer blockExplorer,
            IObservableRepository<string, ObservableWalletEntity> observableWalletRepository)
        {
            _blockExplorer = blockExplorer;
            _observableWalletRepository = observableWalletRepository;
        }
        
        public async Task SubscribeAsync(string address)
        {
            _subscription.Add(address);
        }

        public async Task<bool> UnsubscribeAsync(string address)
        {
            return _subscription.Remove(address);
        }

        /// <summary>
        /// Retrieves balances for all subscribed addresses
        /// </summary>
        /// <param name="take"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public async Task<PaginationResponse<WalletBalanceContract>> GetBalancesAsync(int take, string continuation)
        {
            // TODO: Revisit this to be actually performant.
            // TODO: Make sure the balance value is correctly encoded
            
            var wallets = await _observableWalletRepository.List(take, continuation);
            var balances = new List<WalletBalanceContract>();
            foreach (var wallet in wallets.Items)
            {
                var balance = await _blockExplorer.GetAddressBalance(wallet.Address);
                var balanceContract = new WalletBalanceContract
                {
                    Address = wallet.Address,
                    AssetId = "DCR",
                    Balance = balance.ToString(),
                    Block = 0
                };

                balances.Add(balanceContract);
            }
            
            return new PaginationResponse<WalletBalanceContract> { 
                Items = balances, 
                Continuation = wallets.Continuation
            };
        }
    }
}
