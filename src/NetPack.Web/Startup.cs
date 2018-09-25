using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NetPack.RequireJs;
using System.Collections.Generic;

namespace NetPack.Web
{
    public class Startup
    {

        private readonly List<IFileProvider> _fileProviders = new List<IFileProvider>();

        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
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
            services.AddNetPack((setup) =>
            {
                setup.AddPipeline(pipelineBuilder =>
                {
                    pipelineBuilder.WithHostingEnvironmentWebrootProvider()
                    // Simple processor, that compiles typescript files into js files.
                    .AddTypeScriptPipe(input =>
                    {
                        input.Include("ts/*.ts");
                    }, options =>
                    {
                        options.Target = Typescript.ScriptTarget.ES5;
                        options.Module = Typescript.ModuleKind.AMD;
                        options.InlineSources = true;
                    })
                    // Another processor that combines multiple js files into a single "bundle" file.
                    .AddJsCombinePipe(input =>
                    {
                        input.Include("ts/*.js");
                    }, () => "bundle.js")
                    // Add a require js processor that takes all AMD format javascript files and optimises them using rjs optimiser.
                    .AddRequireJsOptimisePipe(input =>
                    {
                        input.Include("amd/*.js")
                        .Include("js/requireConfig.js");
                    }, options =>
                    {
                        options.GenerateSourceMaps = true;
                        options.Optimizer = Optimisers.none;
                        options.BaseUrl = "amd";
                         // options.
                         //  options.AppDir = "amd";
                         options.Name = "SomePage"; // The name of the AMD module to optimise.
                         options.Out = "built.js"; // The name of the output file.

                         // Here we list the module names
                         //options.Modules.Add(new ModuleInfo() { Name = "ModuleA" });
                         //options.Modules.Add(new ModuleInfo() { Name = "ModuleB" });
                         //  options.Modules.Add(new ModuleInfo() { Name = "SomePage" });
                     })
                     .AddJsMinPipe(input =>
                     {
                         input.Include("js/*.js");
                     })
                    .UseBaseRequestPath("/netpack") // serves all outputs using the specified base request path.
                    .Watch(); // Inputs are monitored, and when changes occur, pipes will automatically re-process.

                });
            });
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
                // app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseNetPack();
            app.UseStaticFiles();

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
