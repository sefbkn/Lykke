using System.Collections.Generic;
using System.Threading.Tasks;

namespace Decred.BlockExplorer
{
    public interface IAddressBalanceRepository
    {
        /// <summary>
        /// Determines the unspent balance of each address at a point in time.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        Task<AddressBalance[]> GetAddressBalancesAsync(long maxBlockHeight, string[] addresses);
    }

    public class AddressBalance
    {
        public string Address { get; set; }
        public long Balance { get; set; }
        public long Block { get; set; }

        public AddressBalance()
        {
        }
    }
}
