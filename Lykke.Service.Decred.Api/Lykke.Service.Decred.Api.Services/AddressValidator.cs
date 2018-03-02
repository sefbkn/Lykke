using System;
using Paymetheus.Decred;
using Paymetheus.Decred.Wallet;

namespace Lykke.Service.Decred.Api.Services
{
    public interface IAddressValidationService
    {
        /// <summary>
        /// Returns whether or not a given address is valid on the current network.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        bool IsValid(string address);
    }

    public class AddressValidationService : IAddressValidationService
    {
        private readonly NetworkSettings _settings;
        
        public AddressValidationService(NetworkSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (settings.Name == null) throw new ArgumentNullException(nameof(settings.Name));
            
            switch (settings.Name)
            {
                case "mainnet":
                case "testnet":
                case "simnet":
                    _settings = settings;
                    break;
                default:
                    throw new ArgumentException("Invalid network");
            }
        }
        
        public bool IsValid(string address)
        {            
            return Address.TryDecode(address, out var addr) && addr.IntendedBlockChain.Name == _settings.Name;            
        }
    }
}
