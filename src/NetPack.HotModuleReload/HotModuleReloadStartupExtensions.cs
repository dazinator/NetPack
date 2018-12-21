
using System;
using Microsoft.AspNetCore.Builder;
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

        public static IApplicationBuilder UseHotModuleReload(this IApplicationBuilder app)
        {
            app.UseSignalR(routes =>
            {
                routes.MapHub<HotModuleReloadHub>(DefaultHubPathString);
            });
            return app;
        }

        public static HubRouteBuilder MapHotModuleReloadHub(this HubRouteBuilder builder, string path = DefaultHubPathString)
        {
            builder.MapHub<HotModuleReloadHub>(path);
            return builder;
        }

    }
}