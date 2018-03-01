using System.Runtime.Serialization;

namespace Lykke.Service.Decred.SignService.Models
{
    [DataContract]
    public class SignedTransactionResponse
    {
        [DataMember(Name = "signedTransaction")]
        public string SignedTransaction { get; set; }
    }
}
