using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace NetPack.Web.Blazor
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBrowserReloadClient((sp) =>
            {
                var urlHelper = sp.GetRequiredService<IUriHelper>();
                var hubUrl = urlHelper.ToAbsoluteUri("reloadhub");
                Console.WriteLine("Browser Reload: " + hubUrl);
                return hubUrl.ToString();                
            });
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
