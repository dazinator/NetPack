using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace NetPack.Web.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static WebAssemblyHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            ConfigureServices(builder.Services);
            return builder;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddBrowserReloadClient((sp) =>
            {
                var urlHelper = sp.GetRequiredService<NavigationManager>();
                var hubUrl = urlHelper.ToAbsoluteUri("reloadhub");
                Console.WriteLine("Browser Reload: " + hubUrl);
                return hubUrl.ToString();
            });
        }
    }
}
