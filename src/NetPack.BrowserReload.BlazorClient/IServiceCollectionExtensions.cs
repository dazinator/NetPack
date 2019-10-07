using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NetPack.BrowserReload.BlazorClient;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddBrowserReloadClient(this IServiceCollection services, Func<IServiceProvider, string> getUrl)
        {                 
            services.AddSingleton<BrowserReloadClient>((sp)=> {
                var url = getUrl(sp);
                var jsRuntime = sp.GetRequiredService<IJSRuntime>();
                var navManager = sp.GetRequiredService<NavigationManager>();
                var client = new BrowserReloadClient(jsRuntime, navManager, url);
                return client;
            });
            return services;
        }
    }
}
