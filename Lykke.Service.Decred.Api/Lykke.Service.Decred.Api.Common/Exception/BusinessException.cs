using System;

namespace Lykke.Service.Decred.Api.Common
{
    public class BusinessException : Exception
    {
        public ErrorReason Reason { get; }

        public BusinessException(ErrorReason reason, string message = "", Exception innerException = null) : base(message, innerException)
        {
            Reason = reason;
        }
    }

    public enum ErrorReason
    {
        DuplicateRecord = 0,
        RecordNotFound = 1
    }
}
