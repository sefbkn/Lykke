using System;

namespace Decred.Common
{
    public class Base58CheckException : Exception
    {
        public Base58CheckException(string message) : base(message)
        {
        }
    }
}
