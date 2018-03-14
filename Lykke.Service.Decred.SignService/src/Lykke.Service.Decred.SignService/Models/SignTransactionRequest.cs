using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Lykke.Service.Decred.SignService.Models
{
    [DataContract]
    public class SignTransactionRequest : IValidatableObject
    {
        /// <summary>
        /// Serialized message containing, at least, the transaction to be signed.
        /// </summary>
        [DataMember(Name = "transactionContext")]
        public string TransactionContext { get; set; }
        
        [DataMember(Name = "privateKeys")]
        public string[] Keys { get; set; }
        
        /// <summary>
        /// Runs validation on this object and returns validation errors, if any.
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(TransactionContext))
                yield return new ValidationResult("Value cannot be null", new[] {"transactionContext"});
            if (Keys == null || !Keys.Any())
                yield return new ValidationResult("Value cannot be null", new[] {"privateKeys"});
        }
    }
}
