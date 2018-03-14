namespace NDecred.Common
{
    public class Base58CheckException : System.Exception
    {
        public Base58CheckException(string message) : base(message)
        {
        }
    }
}
