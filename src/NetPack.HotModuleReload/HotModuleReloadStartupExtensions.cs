
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;
using NetPack.HotModuleReload;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public static class HotModuleReloadStartupExtensions
    {
        private const string DefaultHubPathString = "/hmrhub";

        public static IServiceCollection AddHotModuleReload(this IServiceCollection services, Action<HotModuleReloadOptions> configureOptions)
        {            
            services.AddSingleton<IHostedService, HotModuleReloadHostedService>();           
            services.Configure<HotModuleReloadOptions>(configureOptions);
            services.ConfigureOptions(typeof(PostConfigureStaticFilesOptions));
            return services;
        }
        public static IEndpointRouteBuilder MapHotModuleReloadHub(this IEndpointRouteBuilder endpoints, string path = DefaultHubPathString)
        {
            endpoints.MapHub<HotModuleReloadHub>(path);
            return endpoints;
        }

    }
}