using System.Threading.Tasks;

namespace ManagemeConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var app = new App.App();
            
            await app.InitAsync();

            await app.RunAsync();
        }
    }
}

