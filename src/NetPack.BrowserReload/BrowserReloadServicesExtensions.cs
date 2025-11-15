
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetPack.BrowserReload;
using Microsoft.AspNetCore.SignalR;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public static class BrowserReloadServicesExtensions
    {
        private const string DefaultHubPathString = "/reloadhub";

        public static IServiceCollection AddBrowserReload(this IServiceCollection services, Action<BrowserReloadOptions> configureOptions)
        {            
            services.AddSingleton<IHostedService, BrowserReloadHostedService>();           
            services.Configure<BrowserReloadOptions>(configureOptions);
            services.ConfigureOptions(typeof(BrowserReloadOptionsConfigureOptions));
            return services;
        }
        
        public static IEndpointRouteBuilder MapBrowserReloadHub(this IEndpointRouteBuilder endpoints, string path = DefaultHubPathString)
        {
            endpoints.MapHub<BrowserReloadHub>(path);
            return endpoints;
        }

        // public static IApplicationBuilder UseBrowserReload(this IApplicationBuilder app)
        // {
        //     app.UseSignalR(routes =>
        //     {
        //         routes.MapHub<BrowserReloadHub>(DefaultHubPathString);
        //     });
        //     return app;
        // }

        // public static HubRouteBuilder MapBrowserReloadHub(this HubRouteBuilder builder, string path = DefaultHubPathString)
        // {
        //     builder.MapHub<BrowserReloadHub>(path);
        //     return builder;
        // }

    }
}