using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NetPack;
using NUglify;
using static NetPack.Pipes.Typescript.TypeScriptPipeOptions;
using NetPack.Pipes;

namespace NetPack.Web
{
    public class Startup
    {

        private List<IFileProvider> _fileProviders = new List<IFileProvider>();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddNetPack();
            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            if (!env.IsProduction())
            {
                // add another pipeline that takes outputs from previous pipeline and bundles them


            }

            app.UseFileProcessing(a =>
            {
                a.WithHostingEnvironmentWebrootProvider()
                    // Simple processor, that compiles typescript files.
                    .AddTypeScriptPipe(input =>
                    {
                        input.Include("ts/*.ts");
                    }, options =>
                    {
                        // options.InlineSourceMap = true;
                        options.InlineSources = true;
                        // configure various typescript compilation options here..
                        // options.InlineSourceMap = true;
                        //  options.Module = ModuleKind.Amd;
                    })
                    // Another processor that combines multiple js files into a bundle file.
                    .AddJsCombinePipe(input =>
                    {
                        input.Include("ts/*.js");
                    }, () => "bundle.js")
                    .UseBaseRequestPath("netpack") // serves all outputs using the specified base request path.
                    .Watch(); // Inputs are monitored, and when changes occur, pipes will automatically re-process.
            });

            app.UseMvc(routes =>
             {
                 routes.MapRoute(
                     name: "default",
                     template: "{controller=Home}/{action=SingleTypescriptFile}/{id?}");
             });

            app.Use(async (context, next) =>
            {
                //  await context.Response.WriteAsync("Pre Processing");

                await next();

                //  await context.Response.WriteAsync("Post Processing");
            });
        }

    }
}
