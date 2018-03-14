namespace NDecred.Common
{
    public class SigningException : System.Exception
    {
        public SigningException(string message = null, System.Exception innerException = null) 
            : base(message, innerException)
        {}
    }
}
