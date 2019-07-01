using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NetPack.Blazor;
using System;

namespace NetPack.Web.Blazor.Host
{
    public class Startup
    {
        public IFileProvider BlazorClientWebRootFileProvider { get; private set; }

        public IFileProvider BlazorClientContentFileProvider { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNetPack((a) =>
            {
                a.AddPipeline((b) =>
                {
                    _ = b.WithBlazorClientContentFileProvider<NetPack.Web.Blazor.Startup>()
                    .AddBlazorRecompilePipe<NetPack.Web.Blazor.Startup>()
                    .Watch();
                });
            });

            // Requirement for browser reload.
            services.AddSignalR();
            BlazorClientWebRootFileProvider = BlazorClientAppFileProviderHelper.GetStaticFileProvider<NetPack.Web.Blazor.Startup>();
            services.AddBrowserReload((options) =>
            {
                options.FileProvider(BlazorClientWebRootFileProvider)
                         .Watch("/**/*.dll")
                         .Watch("/**/*.css")
                         .Watch("/**/*.html");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseNetPack();
            app.UseStaticFiles();
            app.UseBrowserReload();

            app.UseClientSideBlazorFiles(BlazorClientWebRootFileProvider, true);
            // platformAdminTenantApp.UseStaticFiles();
            app.UseBrowserReload();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapFallbackToClientSideBlazor<NetPack.Web.Blazor.Startup>("index.html");
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});
            });
        }
    }
}
