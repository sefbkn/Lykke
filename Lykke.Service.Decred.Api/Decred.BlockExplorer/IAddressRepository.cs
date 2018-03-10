using System.Collections.Generic;
using System.Threading.Tasks;

namespace Decred.BlockExplorer
{
    public interface IAddressRepository
    {
        /// <summary>
        /// Determines the unspent balance of each address at a point in time.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        Task<AddressBalance> GetAddressBalanceAsync(long maxBlockHeight, string addresses);
        
        /// <summary>
        /// Returns no more than 'take' unspent transaction hashes,
        /// for the given address, occuring after 'afterHash'
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetUnspentTransactionIds(string address, int take, string afterHash);
    }
}
