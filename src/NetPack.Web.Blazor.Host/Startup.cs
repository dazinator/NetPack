using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NetPack.Blazor;

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
                    .AddProcessPipe((input) =>
                    {
                        input.Include("/**/*.razor");
                    }, (processOptions) =>
                    {
                        processOptions.ExecutableName = "dotnet.exe";
                        var projDir = BlazorClientAppFileProviderHelper.GetBlazorClientProjectDirectory<NetPack.Web.Blazor.Startup>(out string outputAssemblyPath);
                        processOptions.WorkingDirectory = projDir;
                        processOptions.AddArgument("build");
                        Action<string> logOutput = new Action<string>((a) =>
                        {
                            Console.WriteLine(a.ToString());
                        });
                        Action<string> logErrorCallback = new Action<string>((a) =>
                        {
                            Console.WriteLine(a.ToString());
                        });
                        processOptions.StdOutCallback = logOutput;
                        processOptions.StdErrCallback = logErrorCallback;
                    })
                    .UseBaseRequestPath("/netpack")
                    .Watch();
                });
            });

            services.AddSignalR();

            BlazorClientWebRootFileProvider = BlazorClientAppFileProviderHelper.GetStaticFileProvider<NetPack.Web.Blazor.Startup>();
           // BlazorClientContentFileProvider = BlazorClientAppFileProviderHelper.GetContentFileProvider<NetPack.Web.Blazor.Startup>();

            services.AddBrowserReload((options) =>
            {
                options.FileProvider(BlazorClientWebRootFileProvider)
                         .Watch("/**/*.dll")
                         .Watch("/**/*.css")
                         .Watch("/**/*.html");

                //options.FileProvider(BlazorClientContentFileProvider)                       
                //        .Watch("/**/*.razor");


                // options.
                // trigger browser reload when our bundle file changes.
                //options.WatchWebRoot("/**/*.dll");
                //options.WatchWebRoot("/**/*.css");
                //options.WatchWebRoot("/**/*.html");
                // options.WatchContentRoot("/Views/**/*.cshtml");
                // a.WebRootWatchPatterns("");
            });

            //services.AddRazorPages().AddRazorRuntimeCompilation((a) =>
            //{
            //    a.FileProviders.Add(BlazorClientContentFileProvider);
            //});

            //services.AddMvc().AddRazorRuntimeCompilation((a) =>
            //{
            //    a.FileProviders.Add(BlazorClientContentFileProvider);

            //});

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
