using System.Threading.Tasks;

namespace Lykke.Service.Decred_BlockchainApi.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}