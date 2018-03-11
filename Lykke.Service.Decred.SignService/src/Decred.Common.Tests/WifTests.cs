using System;
using Lykke.Service.Decred.SignService.Services;
using NDecred.Cryptography;
using Xunit;

namespace Decred.Common.Tests
{
    public class WifTests
    {
        [Fact]
        public void Wif_GivenPrivateKeyBytes_ReturnsExpectedPublicAddress()
        {
            var tests = new(Network network, string wif, string publicAddress)[]
            {
                (Network.Testnet, "", "")
            };

            foreach (var test in tests)
            {
                var privateKey = Wif.Deserialize(test.network, test.wif);
                var ecService = new ECSecurityService();
                var publicKey = ecService.GetPublicKey(privateKey, true);
                var publicAddress = new Base58Check().Encode(
                    test.network.AddressPrefix.PayToPublicKeyHash, 
                    HashUtil.Ripemd160(HashUtil.Blake256(publicKey)), 
                    false);
            
                Assert.Equal(test.publicAddress, publicAddress);
            }
        }
    }
}
