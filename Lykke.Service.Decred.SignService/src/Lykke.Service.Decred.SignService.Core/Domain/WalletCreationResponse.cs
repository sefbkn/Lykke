using System.Runtime.Serialization;

namespace Lykke.Service.Decred.SignService.Core.Services
{
    [DataContract]
    public class WalletCreationResponse
    {
        [DataMember(Name = "publicAddress")]
        public string PublicAddress { get; set; }

        [DataMember(Name = "privateKey")]
        public string PrivateKey { get; set; }
    }
}
