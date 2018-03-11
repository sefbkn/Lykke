using System;
using Lykke.Service.Decred.SignService.Services;
using NDecred.Cryptography;
using Xunit;

namespace Decred.Common.Tests
{
    public class WifTests
    {
        [Fact]
        public void Wif_GivenPrivateKeyBytes_CanBuildExpectedPublicAddress()
        {
            // Don't use these addresses...
            var tests = new(Network network, string wif, string publicAddress, bool valid)[]
            {
                (Network.Testnet, 
                "PtWTw6fqGqkzEgaHE2KF6gBRUPktyLwBjAMoUjtsAc6e9qkdkbYPs", 
                "TscoEp7o1fs4jfwDBP4mCad5pJZQ7HLK2tu", true),

                (Network.Testnet, 
                "PtWV8M3FJ4eB3XN92zD9m8g3JHSB7iMLQ5xFoFwzoWEFY2BtBvUmv", 
                "TsSMdW2j9v9otWbiBdVg4B1HarPJSTY4fyj", true),
                

                (Network.Testnet, 
                "PmQe4Ms6AX8JCi6eY8sxoDVC3z73ymHukywt4dtAqXzXqFkpSiHzS", 
                "DseCUEJzEAMXYSrM9BowB8jAEXt6sdBYzBA", false),

                (Network.Mainnet, 
                "PmQejS1Vtt29hhy7nNP1aHoqqourC4Vox5kGGL39ddhHETTyvqwZg", 
                "Dskk1zBVkkmn2MUTAkkAz4pTuF1Bn8QBUrZ", true),

                (Network.Mainnet, 
                "PmQdNHBhKn9F6XcjjP1SPSmQhVUmdHtjSuyMiTjzJkT5H88WLJi6s", 
                "DsjYjz5nxM7uYoHShimxi9EsJJvnp8V83ai", true),

                (Network.Mainnet, 
                "PmQdf8P4Nj4v163g9R6NaSHMinaLAeidsWPyXg53sCtPghYsApVcW", 
                "DseCUEJzEAMXYSrM9BowB8jAEXt6sdBYzBS", false),
            };

            foreach (var test in tests)
            {
                try
                {
                    var privateKey = Wif.Deserialize(test.network, test.wif);
                    var ecService = new ECSecurityService();
                    var publicKey = ecService.GetPublicKey(privateKey, true);
                    var publicAddress = new Base58Check().Encode(
                        test.network.AddressPrefix.PayToPublicKeyHash, 
                        HashUtil.Ripemd160(HashUtil.Blake256(publicKey)), 
                        false);

                    Assert.Equal(test.valid, test.publicAddress == publicAddress);
                }
                catch (WifException e)
                {
                    // If the test is supposed to be valid
                    // rethrow the exception.
                    if (test.valid)
                        throw;
                }
            }
        }
    }
}
