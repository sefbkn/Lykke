using System;

namespace Lykke.Service.Decred.Api.Services
{
    public interface IAddressValidationService
    {
        bool IsValid(string address);
    }

    /// <summary>
    /// Determines if the structure of an address is valid for the current network.
    /// </summary>
    public class AddressValidationService : IAddressValidationService
    {
        public bool IsValid(string address)
        {
            throw new NotImplementedException();
        }
    }
}
