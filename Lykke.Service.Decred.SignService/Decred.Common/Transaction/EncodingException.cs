using System;
using System.Text;

namespace Decred.Common
{
    public class EncodingException : Exception
    {
        public EncodingException(string message = null, Exception innerException = null) : base(message,innerException)
        {}
    }
}
