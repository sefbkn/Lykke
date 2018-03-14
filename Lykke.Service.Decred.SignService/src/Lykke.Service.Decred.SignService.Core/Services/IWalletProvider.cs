using System.Runtime.Serialization;

namespace Lykke.Service.Decred_SignService.Core.Services
{
    public interface IWalletProvider
    {
        WalletCreationResponse CreateNewWallet();
    }
    
    [DataContract]
    public class WalletCreationResponse
    {
        [DataMember(Name = "publicAddress")]
        public string PublicAddress { get; set; }

        [DataMember(Name = "privateKey")]
        public string PrivateKey { get; set; }
    }
}
