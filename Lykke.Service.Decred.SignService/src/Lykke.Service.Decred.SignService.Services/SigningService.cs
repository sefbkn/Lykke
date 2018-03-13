using System;
using System.Linq;
using Decred.Common;
using Lykke.Service.Decred.SignService.Services;
using NDecred.Common;
using Paymetheus.Decred;
using Paymetheus.Decred.Script;

namespace Lykke.Service.Decred_SignService.Services
{
    public class SigningService
    {
        private readonly ECSecurityService _securityService;
        private readonly Network _network;

        public SigningService(ECSecurityService securityService, Network network)
        {
            _securityService = securityService;
            _network = network;
        }
        
        public string SignRawTransaction(string[] privateKeys, string rawTransaction)
        {
            // Transaction encoded in hex.
            var txBytes = Hex.ToByteArray(rawTransaction);
            var transaction = Transaction.Deserialize(txBytes);
            
            // Go through each input and extract the signature script.
            // Convert each private key to a pubkeyhash
            var pubKeyHashes = privateKeys.Select(privateKey => new
            {
                PrivateKey = GetPrivateKey(privateKey),
                PublicKeyHash = GetPublicKeyHash(privateKey)
            }).ToList();

            // Match up all private keys to the inputs they unlock.
            var query =
                from input in transaction.Inputs.Select((value, index) => (value: value, index: index))
                let outputScript = OutputScript.ParseScript(input.value.SignatureScript)
                    as OutputScript.Secp256k1PubKeyHash
                where outputScript != null
                from pubKeyHash in pubKeyHashes
                where pubKeyHash.PublicKeyHash.SequenceEqual(outputScript.Hash160)
                select new
                {
                    input, outputScript, pubKeyHash
                };


            foreach (var output in transaction.Outputs)
            {
            }
            
            _securityService.Sign(null, null);
            
            throw new Exception();
        }

        private byte[] GetPrivateKey(string wif)
        {
            return Wif.Deserialize(_network, wif);
        }
        
        private byte[] GetPublicKeyHash(string wif)
        {
            var privateKey = GetPrivateKey(wif);
            var publicKey = _securityService.GetPublicKey(privateKey, true);
            return HashUtil.Ripemd160(HashUtil.Blake256(publicKey));
        }
    }
}
