using System;

namespace Decred.Common
{
    public class WifException : Exception
    {
        public WifException(string message) : base(message)
        {
        }
    }
}
