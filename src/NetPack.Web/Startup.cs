using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NetPack;
using NetPack.RequireJs;

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
                        options.InlineSources = true;
                    })
                    // Another processor that combines multiple js files into a bundle file.
                    .AddJsCombinePipe(input =>
                    {
                        input.Include("ts/*.js");
                    }, () => "bundle.js")

                     // Add a processor that takes all AMD javascript files and optimises them using rjs optimiser.
                     .AddRequireJsOptimisePipe(input =>
                     {
                         input.Include("amd/*.js");
                     }, options =>
                     {
                         options.GenerateSourceMaps = true;
                         options.Optimizer = Optimisers.none;
                         options.BaseUrl = ".";
                       //  options.AppDir = "amd";
                         options.Name = "amd/SomePage"; // The name of the AMD module to optimise.
                         options.Out = "built.js"; // The name of the output file.
                         
                         // Here we list the module names
                         //options.Modules.Add(new ModuleInfo() { Name = "ModuleA" });
                         //options.Modules.Add(new ModuleInfo() { Name = "ModuleB" });
                         //  options.Modules.Add(new ModuleInfo() { Name = "SomePage" });
                     })

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
