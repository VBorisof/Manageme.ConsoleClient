namespace ManagemeConsoleClient.Forms
{
    public class LoginForm
    {
        public string Name { get; set; }
        public string Password { get; set; }

        public LoginForm(string name, string password)
        {
            Name = name;
            Password = password;
        }
    }
}
