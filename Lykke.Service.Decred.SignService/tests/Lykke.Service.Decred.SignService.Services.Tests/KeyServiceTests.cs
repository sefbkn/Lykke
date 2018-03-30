using System;
using System.Text;
using NDecred.Common;
using NDecred.Common.Wallet;
using Paymetheus.Decred.Wallet;
using Xunit;

namespace Lykke.Service.Decred.SignService.Services.Tests
{
    public class KeyServiceTests
    {
        [Fact]
        public void Create_GeneratesCorrectPublicKey()
        {            
            var networks = new[]{ Network.Testnet, Network.Mainnet };
            foreach (var network in networks)
            {
                var securityService = new SecurityService();
                var keyService = new KeyService(securityService, network);
                var keypair = keyService.Create();
            
                Assert.NotNull(keypair.PrivateKey);
                Assert.NotNull(keypair.PublicAddress);
                
                // Make sure public key is for correct network
                var address = Address.Decode(keypair.PublicAddress);
                Assert.Equal(network.Name, address.IntendedBlockChain.Name);
            }
        }

        [Fact]
        public void SignService_GivenPrivateKey_CanSignAndVerify()
        {
            var testBytes = Encoding.UTF8.GetBytes("test message");
            
            var networks = new[]{ Network.Testnet, Network.Mainnet };
            foreach (var network in networks)
            {
                var securityService = new SecurityService();
                var keyService = new KeyService(securityService, network);
                var keypair = keyService.Create();

                // Make sure public key 
                var privateKeyBytes = Wif.Deserialize(network, keypair.PrivateKey);
                var publicKeyBytes = securityService.GetPublicKey(privateKeyBytes, true);

                var signature = securityService.Sign(privateKeyBytes, testBytes);
                Assert.True(securityService.VerifySignature(publicKeyBytes, testBytes, signature));
            }
        }
    }
}
