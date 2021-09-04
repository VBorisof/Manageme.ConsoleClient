using System;
using System.Threading.Tasks;
using ManagemeConsoleClient.Exceptions;
using ManagemeConsoleClient.Forms;

namespace ManagemeConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var app = new App.App();
           
            try
            {
                await app.InitAsync(new LoginForm("Seva", "123"));
            }
            catch (AppException e)
            {
                Console.WriteLine(
                    "\nFailed to log in! "
                    + "Please make sure the login details are correct.\n"
                    + $"Exception details:\n---------------------------\n{e}");
                return;
            }

            await app.RunAsync();
        }
    }
}

