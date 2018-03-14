using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using Decred.Common;
using Lykke.Service.Decred.SignService.Services;
using NDecred.Common;
using Paymetheus.Decred;
using Paymetheus.Decred.Script;

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
        private readonly ECSecurityService _securityService;
        private readonly Network _network;

        public SigningService(ECSecurityService securityService, Network network)
        {
            _securityService = securityService;
            _network = network;
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
        public string SignRawTransaction(string[] privateKeys, byte[] rawTxBytes)
        {   
            // Deserialize wifs and generate privatekey/publickey/pubkeyhash mappings
            var keys =
               (from wif in privateKeys
                let privKey = Wif.Deserialize(_network, wif)
                select ExpandPrivateKey(privKey)).ToArray();

            // This is the transaction that will have properly signed inputs.
            var transaction = DecodeTransaction(rawTxBytes);
            
            foreach (var input in transaction.TxIn)
            {
                // Clone the base transaction
                var txCopy = DecodeTransaction(rawTxBytes);

                // Match the private key with the public key script for this input
                // Note: the public key script is embedded in the signature script portion of the transaction.
                var publicKeyHash = GetPublicKeyHash(input.SignatureScript);
                var key = keys.Single(k => k.PublicKeyHash.SequenceEqual(publicKeyHash));

                // Zero out all scripts except the current one.
                foreach (var txCopyIn in txCopy.TxIn)
                {
                    // Skip the current TxIn
                    if (input.PreviousOutPoint.Hash.SequenceEqual(txCopyIn.PreviousOutPoint.Hash) &&
                        input.PreviousOutPoint.Index == txCopyIn.PreviousOutPoint.Index) continue;
                    
                    txCopyIn.SignatureScript = new byte[0];                  
                }
                
                // Calculate the tx signature for this input,
                // and sign the hash
                var txHash = CalculateTxHash(txCopy);
                var signature = _securityService.Sign(key.PrivateKey, txHash).MakeCanonical().ToDer();
                var sigBytes = signature.Concat(new[]{(byte) SignatureHashType.All}).ToArray();

                input.SignatureScript = GetSignatureScript(sigBytes, key.PublicKey);
            }

            return Hex.FromByteArray(transaction.Encode());
        }

        private static byte[] GetSignatureScript(byte[] signature, byte[] publicKey)
        {
            using (var ms = new MemoryStream(signature.Length + publicKey.Length + 2))
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteVariableLengthBytes(signature);
                bw.WriteVariableLengthBytes(publicKey);
                return ms.ToArray();
            }
        }

        private static byte[] GetPublicKeyHash(byte[] rawPkScript)
        {
            // Only support Secp256k1 signatures
            var outScript = OutputScript.ParseScript(rawPkScript);
            if (!(outScript is OutputScript.Secp256k1PubKeyHash parsedScript))
                throw new TransactionSigningException("Unsupported signature script type");

            return parsedScript.Hash160;
        }
        
        private static MsgTx DecodeTransaction(byte[] rawTransaction)
        {
            var msgTx = new MsgTx();
            msgTx.Decode(rawTransaction);
            return msgTx;
        }

        public byte[] CalculateTxHash(MsgTx transaction)
        {
            var wbuf = new List<byte>(32 * 2 + 4);
            wbuf.AddRange(BitConverter.GetBytes((uint) 1));

            var prefixHash = transaction.GetHash(TxSerializeType.NoWitness);
            var witnessHash = transaction.GetHash(TxSerializeType.WitnessSigning);

            wbuf.AddRange(prefixHash);
            wbuf.AddRange(witnessHash);

            return HashUtil.Blake256(wbuf.ToArray());
        }

        /// <summary>
        /// Maps a private key to a compressed public key + compressed public key hash
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        private (byte[] PrivateKey, byte[] PublicKey, byte[] PublicKeyHash) ExpandPrivateKey(byte[] privateKey)
        {
            var ecService = new ECSecurityService();
            var publicKey = ecService.GetPublicKey(privateKey, true);
            var publicKeyHash = HashUtil.Ripemd160(HashUtil.Blake256(publicKey));
            return (privateKey, publicKey, publicKeyHash);
        }
    }    
}
