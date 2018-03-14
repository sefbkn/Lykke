using System.Security.Cryptography;
using NDecred.Common.Wallet;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;

namespace NDecred.Common
{
    public interface ISecurityService
    {
        /// <summary>
        /// Generates a random private key
        /// </summary>
        /// <returns></returns>
        byte[] NewPrivateKey();
        
        /// <summary>
        /// Generates a public key given a private key and compression option
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="isCompressed"></param>
        /// <returns></returns>
        byte[] GetPublicKey(byte[] privateKey, bool isCompressed);
        
        /// <summary>
        /// Generates a signature for a given set of data with the
        /// provided private key.
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Signature Sign(byte[] privateKey, byte[] data);
        
        /// <summary>
        /// Verifies the signature of a message against a given public key
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="message"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        bool VerifySignature(byte[] publicKey, byte[] message, Signature signature);
    }
    
    /// <summary>
    /// Provides methods for 
    /// </summary>
    public class SecurityService : ISecurityService
    {
        private const int PrivateKeyLength = 32;
        private static readonly RNGCryptoServiceProvider CRandom = new RNGCryptoServiceProvider();

        private IDigest DigestAlgorithm => new Sha256Digest();
        private X9ECParameters CurveParameters => CustomNamedCurves.GetByOid(SecObjectIdentifiers.SecP256k1);
        private ECDomainParameters DomainParameters => GetEllipticCurveDomainParameters(CurveParameters);

        private readonly BigInteger negativeOne = BigInteger.One.Negate();
        
        public byte[] NewPrivateKey()
        {
            var bytes = new byte[PrivateKeyLength];
            CRandom.GetBytes(bytes);
            
            // Keep value between 1 and N-1
            var bigint = new BigInteger(bytes);
            var orderG = CurveParameters.N.Add(negativeOne);
            bigint.Mod(orderG).Add(BigInteger.One);
            
            return bigint.ToByteArray();
        }
        
        public byte[] GetPublicKey(byte[] privateKey, bool isCompressed)
        {
            var privateKeyParameters = GetPrivateKeyParameters(privateKey);
            var publicKeyParameters = GetPublicKeyParameters(DomainParameters, privateKeyParameters);
            var q = publicKeyParameters.Q.Normalize();
            return DomainParameters.Curve
                .CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded(isCompressed);
        }
        
        public Signature Sign(byte[] privateKey, byte[] data)
        {
            var privateKeyParameters = GetPrivateKeyParameters(privateKey);

            var ecdsaSigner = new ECDsaSigner(new HMacDsaKCalculator(DigestAlgorithm));
            ecdsaSigner.Init(true, privateKeyParameters);
            var signature = ecdsaSigner.GenerateSignature(data);
            return new Signature(signature[0], signature[1]);
        }
        
        public bool VerifySignature(byte[] publicKey, byte[] message, Signature signature)
        {
            var ecPoint = CurveParameters.Curve.DecodePoint(publicKey);
            var publicKeyParameters = new ECPublicKeyParameters("EC", ecPoint, DomainParameters);
            var ecdsaSigner = new ECDsaSigner(new HMacDsaKCalculator(DigestAlgorithm));
            ecdsaSigner.Init(false, publicKeyParameters);
            
            return ecdsaSigner.VerifySignature(message, signature.R, signature.S);
        }
        
        private ECPublicKeyParameters GetPublicKeyParameters(ECDomainParameters domainParameters,
            ECPrivateKeyParameters privateKeyParameters)
        {
            var q = domainParameters.G.Multiply(privateKeyParameters.D);
            return new ECPublicKeyParameters(q, DomainParameters);
        }

        private ECPrivateKeyParameters GetPrivateKeyParameters(byte[] secret)
        {
            var secretBytes = new BigInteger(1, secret);
            return new ECPrivateKeyParameters(secretBytes, DomainParameters);
        }

        private ECDomainParameters GetEllipticCurveDomainParameters(X9ECParameters parameters)
        {
            return new ECDomainParameters(parameters.Curve, parameters.G, parameters.N, parameters.H);
        }
    }
}
