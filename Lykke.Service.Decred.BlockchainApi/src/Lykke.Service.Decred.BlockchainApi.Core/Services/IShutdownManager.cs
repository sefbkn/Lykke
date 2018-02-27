using System.Threading.Tasks;
using Common;

namespace Lykke.Service.Decred_BlockchainApi.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();

        void Register(IStopable stopable);
    }
}
