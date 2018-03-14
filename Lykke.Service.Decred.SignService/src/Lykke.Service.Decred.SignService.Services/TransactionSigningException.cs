using System;

namespace Lykke.Service.Decred.SignService.Services
{
    public class TransactionSigningException : Exception
    {
        public TransactionSigningException(string message = null, Exception innerException = null) 
            : base(message, innerException)
        {}
    }
}
