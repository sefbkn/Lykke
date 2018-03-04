using System;

namespace Lykke.Service.Decred.Api.Common
{
    public class BusinessException : Exception
    {
        public BusinessException(string message = "", Exception innerException = null) : base(message, innerException)
        {
        }
    }
    
}
