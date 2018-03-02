using System;
using Lykke.Service.BlockchainApi.Contract.Balances;

namespace Lykke.Service.Decred.Api.Services
{
    public class AddressBalance : WalletBalanceContract
    {        
        public DateTime Timestamp { get; set; }
    }
}