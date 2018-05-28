using NDecred.Common;
using NDecred.Common.Wallet;
using Xunit;

namespace Lykke.Service.Decred.SignService.Services.Tests
{
    public class SigningServiceTests
    {
        // Sample testnet keys
        private string[] _keys =
        {
            "PtWTdZuQXrRgH3GMEaAMv1oBgipQoBPRQD13oS27tBugrmS9SMe5N",
            "PtWUfR8YXe4r8VGdmPpBTktkTXTLghX5yS5DE4Dy17qqHbXwvPDXW"
        };

        [Fact]
        public void SignRawTransaction_GivenUnsignedTx_ReturnsCorrectlySignedTx()
        {
            var unsignedHexTx =
                "010000000198c61f42bf153b869557f9dcb5b95da114b6267ed3d17d7dffddb8ee7cb2ec080100000000" +
                "ffffffff02a08601000000000000001976a9146e0ece55286e20b787c47c69b528ed86c7de315588acc0" +
                "08ff050000000000001976a9146e0ece55286e20b787c47c69b528ed86c7de315588ac000000000000000" +
                "001001602060000000070fd0300010000001976a9146e0ece55286e20b787c47c69b528ed86c7de315588" +
                "ac00000000";

            var signedHexTx = 
                "010000000198c61f42bf153b869557f9dcb5b95da114b6267ed3d17d7dffddb8ee7cb2ec080100000000" +
                "ffffffff02a08601000000000000001976a9146e0ece55286e20b787c47c69b528ed86c7de315588acc00" +
                "8ff050000000000001976a9146e0ece55286e20b787c47c69b528ed86c7de315588ac0000000000000000" +
                "01001602060000000070fd0300010000006b483045022100f05de47bdaf5ec113716735e847b466e26c7d" +
                "fc86db1e3057c4d660fefac1f8e02202ecf1ad50c07c249c6b1f1bca06cfc5b91354bf51e8a344712e0c0" +
                "0277d47c4701210230293c8e9447ff8179eded8541a4079dc11a7d2b67742179e31648e2d7256a95";

            var network = Network.Testnet;
            var securityService = new SecurityService();
            var signingWallet = new SigningWallet(network, securityService);
            var signingService = new SigningService(signingWallet);

            var result = signingService.SignRawTransaction(_keys, HexUtil.ToByteArray(unsignedHexTx));
            
            Assert.Equal(signedHexTx, result);
        }

        [Fact]
        public void SignRawTransaction_WithUnknownPrivateKey_ThrowsException()
        {
            var unsignedHexTx =
                "010000000198c61f42bf153b869557f9dcb5b95da114b6267ed3d17d7dffddb8ee7cb2ec080100000000" +
                "ffffffff02a08601000000000000001976a9146e0ece55286e20b787c47c69b528ed86c7de315588acc0" +
                "08ff050000000000001976a9146e0ece55286e20b787c47c69b528ed86c7de315588ac000000000000000" +
                "001001602060000000070fd0300010000001976a9146e0ece55286e20b787c47c69b528ed86c7de315588" +
                "ac00000000";

            var network = Network.Testnet;
            var securityService = new SecurityService();
            var signingWallet = new SigningWallet(network, securityService);
            var signingService = new SigningService(signingWallet);

            Assert.Throws<SigningException>(() => 
                signingService.SignRawTransaction(new string[0], HexUtil.ToByteArray(unsignedHexTx))
            );
        }
    }
}
