using System.Threading.Tasks;
using Lykke.Service.Decred.SignService.Models;

namespace Lykke.Service.Decred.SignService.Services
{
    public interface ITransactionService
    {
        /// <summary>
        /// Given a raw, unsigned transaction and a set of keys,
        /// sign the transaction.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SignedTransactionResponse> SignAsync(SignTransactionRequest request);
    }
}