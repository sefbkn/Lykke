using System.Linq;
using System.Threading.Tasks;

namespace Decred.BlockExplorer
{
    public abstract class BlockExplorer
    {        
        public abstract Task<AddressTxRaw[]> GetAddressTxRawAsync(string address, int? count = 0);

        public async Task<decimal> GetAddressBalance(string address)
        {
            var txs = await GetAddressTxRawAsync(address);
            return txs.SelectMany(t => t.Vout).Sum(v => v.Value);
        }
    }
}
