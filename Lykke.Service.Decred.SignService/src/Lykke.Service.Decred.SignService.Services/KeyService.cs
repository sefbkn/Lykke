using System;
using System.Security.Cryptography;
using Lykke.Service.Decred.SignService.Core.Services;
using Lykke.Service.Decred.SignService.Services;
using Lykke.Service.Decred_SignService.Core.Services;
using NDecred.Common;

namespace Lykke.Service.Decred.SignService.Services
{
    /// <summary>
    /// Creates keys that can be used in transactions.
    /// </summary>
    public class KeyService : IKeyService
    {
        private readonly Network _network;
        private readonly SecurityService _securityService;

        public KeyService(SecurityService securityService, Network network)
        {
            _securityService = securityService;
            _network = network;
        }
        
        public WalletCreationResponse Create()
        {
            var privateKey = _securityService.NewPrivateKey();
            var publicKey = _securityService.GetPublicKey(privateKey, true);
            
            return new WalletCreationResponse
            {
                PrivateKey = GetWif(privateKey),
                PublicAddress = GetPublicAddress(publicKey)
            };
        }

        private string GetWif(byte[] privateKey)
        {
            return Wif.Serialize(_network, ECDSAType.ECTypeSecp256k1, false, privateKey);
        }

        private string GetPublicAddress(byte[] publicKey)
        {
            var prefix = _network.AddressPrefix.PayToPublicKeyHash;
            var pubKeyHash = HashUtil.Ripemd160(HashUtil.Blake256(publicKey));
            return new Base58Check().Encode(prefix, pubKeyHash, false);
        }
    }
}
