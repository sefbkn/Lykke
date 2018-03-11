using System;
using System.Security.Cryptography;
using Decred.Common;
using Lykke.Service.Decred.SignService.Services;
using Lykke.Service.Decred_SignService.Core.Services;
using NDecred.Cryptography;

namespace Lykke.Service.Decred_SignService.Services
{
    public class WalletProvider : IWalletProvider
    {
        private const int PrivateKeyLength = 32;
        private static readonly RNGCryptoServiceProvider CRandom = new RNGCryptoServiceProvider();

        private readonly Network _network;
        private readonly ECSecurityService _securityService;

        public WalletProvider(ECSecurityService securityService, Network network)
        {
            _securityService = securityService;
            _network = network;
        }
        
        public WalletCreationResponse CreateNewWallet()
        {
            var privateKey = GetRandomBytes();
            var publicKey = _securityService.GetPublicKey(privateKey, true);
            
            return new WalletCreationResponse
            {
                PrivateKey = Wif.Serialize(_network, ECDSAType.ECTypeSecp256k1, false, privateKey),
                PublicAddress = GetPublicAddress(publicKey)
            };
        }

        private string GetPublicAddress(byte[] publicKey)
        {
            var prefix = _network.AddressPrefix.PayToPublicKeyHash;
            var pubKeyHash = HashUtil.Ripemd160(HashUtil.Blake256(publicKey));
            return new Base58Check().Encode(prefix, pubKeyHash, false);
        }

        public virtual byte[] GetRandomBytes()
        {
            var bytes = new byte[PrivateKeyLength];
            CRandom.GetBytes(bytes);
            return bytes;
        }
    }
}
