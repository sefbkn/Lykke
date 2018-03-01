using System;
using System.Collections.Generic;

namespace Lykke.Service.Decred.Api.Services
{
    public interface IAddressService
    {
        void Subscribe(string address);
        void Unsubscribe(string address);
        (IEnumerable<PublicAddress> addresses, string continuation) GetBalances(int take, string continuation);
    }
    
    public class AddressService : IAddressService
    {
        public void Subscribe(string address)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(string address)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves balances for all observed addresses given paging constraints.
        /// </summary>
        /// <param name="take"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public (IEnumerable<PublicAddress> addresses, string continuation) GetBalances(int take, string continuation)
        {
            throw new NotImplementedException();
        }
    }
}
