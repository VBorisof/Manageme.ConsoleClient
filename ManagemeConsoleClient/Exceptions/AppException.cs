using System;

namespace ManagemeConsoleClient.Exceptions
{
    public class AppException : Exception
    {
        public AppException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
