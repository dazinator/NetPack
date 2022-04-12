
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetPack.BrowserReload;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Routing;

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

        public static HubEndpointConventionBuilder UseBrowserReload(this IEndpointRouteBuilder app)
        {
            return app.MapHub<BrowserReloadHub>(DefaultHubPathString);

        }

        //public static HubRouteBuilder MapBrowserReloadHub(this HubRouteBuilder builder, string path = DefaultHubPathString)
        //{
        //    builder.MapHub<BrowserReloadHub>(path);
        //    return builder;
        //}

    }
}