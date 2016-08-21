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
using NetPack.Pipes.Typescript;

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
            services.AddMvc();
            services.AddNetPack();
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

            app.UseStaticFiles("");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=SingleTypescriptFile}/{id?}");
            });

            CompileIndividualTypescriptFiles(app);
            CompileIndividualTypescriptFilesThenCombineThem(app);


            _fileProviders.Add(env.WebRootFileProvider);
            env.WebRootFileProvider = new CompositeFileProvider(_fileProviders);

            if (!env.IsProduction())
            {
                // add another pipeline that takes outputs from previous pipeline and bundles them


            }

        }

        private void CompileIndividualTypescriptFiles(IApplicationBuilder app)
        {
            var contentPipeline = app.UseContentPipeLine(pipelineBuilder =>
             {
                 return pipelineBuilder
                    .Take(files => files
                                    .Include("wwwroot/ts/Another.ts")
                                    .Include("wwwroot/ts/Greeter.ts"))
                                    .Watch()
                    .BeginPipeline()
                        .AddTypeScriptPipe(tsConfig =>
                        {
                            tsConfig.Target = TypeScriptPipeOptions.ScriptTarget.Es5;
                            tsConfig.Module = TypeScriptPipeOptions.ModuleKind.CommonJs;
                            tsConfig.NoImplicitAny = true;
                            tsConfig.RemoveComments = false;
                            tsConfig.SourceMap = true;
                        })
                     .BuildPipeLine();
             })
             .UsePipelineOutputAsStaticFiles();

            // Can access useful properties of the pipeline here like the FileProvider used to serve outputs from the pipeline.
            //  var pipelineOutputsFileProvider = contentPipeline.PipelineFileProvider;
            //  var existingFileProvider = environment.WebRootFileProvider;
            // var allFiles = existingFileProvider.GetDirectoryContents("");

            _fileProviders.Add(contentPipeline.PipelineFileProvider);

        }


        private void CompileIndividualTypescriptFilesThenCombineThem(IApplicationBuilder app)
        {
            var contentPipeline = app.UseContentPipeLine(pipelineBuilder =>
            {
                return pipelineBuilder
                   .Take(files => files
                                   .Include("wwwroot/ts/Another.ts")
                                   .Include("wwwroot/ts/Greeter.ts"))
                                   .Watch()
                   .BeginPipeline()
                       .AddTypeScriptPipe(tsConfig =>
                       {
                           tsConfig.Target = TypeScriptPipeOptions.ScriptTarget.Es5;
                           tsConfig.Module = TypeScriptPipeOptions.ModuleKind.CommonJs;
                           tsConfig.NoImplicitAny = true;
                           tsConfig.RemoveComments = false;
                           tsConfig.SourceMap = true;

                       })
                       .AddCombinePipe(combineConfig =>
                       {
                           combineConfig.CombinedJsFileName = "wwwroot/js/bundleA.js";
                       })
                    .BuildPipeLine();
            })
              .UsePipelineOutputAsStaticFiles();

            _fileProviders.Add(contentPipeline.PipelineFileProvider);
        }




    }
}
