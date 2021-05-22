namespace ManagemeConsoleClient.Exceptions
{
    public class NotLoggedInException : AppException
    {
        public NotLoggedInException() : base("Not logged in", null) { }
    }
}
