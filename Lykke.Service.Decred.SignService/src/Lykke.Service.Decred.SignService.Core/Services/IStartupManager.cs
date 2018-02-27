using System.Threading.Tasks;

namespace Lykke.Service.Decred_SignService.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}