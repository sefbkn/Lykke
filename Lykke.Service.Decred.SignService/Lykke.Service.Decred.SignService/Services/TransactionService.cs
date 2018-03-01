using System.Threading.Tasks;
using Lykke.Service.Decred.SignService.Models;

namespace Lykke.Service.Decred.SignService.Services
{
    public class TransactionService : ITransactionService
    {
        public Task<SignedTransactionResponse> SignAsync(SignTransactionRequest request)
        {
            // TODO: Call dcrwallet to sign
            throw new System.NotImplementedException();
        }
    }
}
