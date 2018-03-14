using NDecred.Common.Wallet;

namespace Lykke.Service.Decred.SignService.Services
{
    public interface ISigningService
    {
        string SignRawTransaction(string[] privateKeys, byte[] rawTxBytes);
    }
    
    /// <summary>
    /// Signs raw transactions
    /// </summary>
    public class SigningService : ISigningService
    {
        private readonly ISigningWallet _signingWallet;

        public SigningService(ISigningWallet signingWallet)
        {
            _signingWallet = signingWallet;
        }
        
        /// <summary>
        /// Given a collection of private keys and a raw transaction
        /// 
        /// * Each input must have the public key script from the respective output
        ///     assigned to the signature script.
        /// </summary>
        /// <param name="privateKeys"></param>
        /// <param name="rawTxBytes"></param>
        /// <returns></returns>
        public string SignRawTransaction(string[] privateKeys, byte[] rawTransaction)
        {
            return _signingWallet.SignRawTransaction(privateKeys, rawTransaction);
        }
    }
}
