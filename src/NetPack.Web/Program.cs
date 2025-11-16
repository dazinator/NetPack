using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NetPack.Utils;

namespace NetPack.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NodeFnmHelper.SetPath();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging((a, b) =>
                {
                    b.AddSimpleConsole();
                    b.AddDebug();
                })
                //.UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
