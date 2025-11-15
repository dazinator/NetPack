using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetPack.RequireJs;

namespace NetPack.Web
{
    public class Startup
    {

        public Startup(IWebHostEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Environment = env;
        }

        public IConfigurationRoot Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddNetPack((setup) =>
            {
                setup.AddPipeline(pipelineBuilder =>
                {

                    var builder = pipelineBuilder.WithHostingEnvironmentWebrootProvider()
                        // Simple processor, that compiles typescript files into js files.                  
                        .AddTypeScriptPipe(input =>
                        {
                            input.Include("ts/*.ts");
                        }, options =>
                        {
                            options.Target = Typescript.ScriptTarget.ES5;
                            options.Module = Typescript.ModuleKind.AMD;
                            options.InlineSources = false;
                            options.InlineSourceMap = false;
                            options.NoImplicitAny = true;
                            options.SourceMap = true;
                        })
                        // Add a require js processor that takes all AMD format javascript files and optimises them using rjs optimiser.
                        .AddRequireJsOptimisePipe(input =>
                        {
                            input.Include("amd/*.js")
                                .Include("js/requireConfig.js");
                        }, options =>
                        {
                            options.MainConfigFile = "/js/requireConfig.js";
                            options.GenerateSourceMaps = true;
                            options.Optimizer = Optimisers.none;
                            options.BaseUrl = "amd";
                            //  options.AppDir = "amd";
                            options.Name = "SomePage"; // The name of the AMD module to optimise.
                            options.Out = "built.js"; // The name of the output file.                        
                        })
                        .AddJsMinPipe(input =>
                        {
                            input.Include("js/*.js");
                        })
                        .AddRollupPipe(input =>
                        {
                            input.Include("amd/*.js")
                                .Include("js/requireConfig.js");
                        }, options =>
                        {
                            // we add a rollup plugin which converts AMD files to ES2016 modules, so they can be included in a rollup bundle.
                            // https://www.npmjs.com/package/rollup-plugin-amd
                            options
                                .ImportModuleLookupAmd() // imports lookup-amd into rollup script so plugins can use lookup() to lookup amd modules.   
                                .AddPluginAmd((amdPluginOptions) =>
                                {
                                    amdPluginOptions.RewireFunction(
                                        "function (moduleId, parentPath) { return lookup({ partial: moduleId, filename: parentPath, config: {baseUrl: '/amd'} }); }");
                                })
                                .HasInput((inputOptions) =>
                                {
                                    inputOptions.Input = "/amd/SomePage.js";
                                })
                                .HasOutput((output) =>
                                {
                                    output.Format = Rollup.RollupOutputFormat.Iife;
                                    output.File = "rollupbundle.js";
                                });
                        })
                        // rollup code splitting example.
                        .AddRollupCodeSplittingPipe(input =>
                        {
                            input.Include("esm/**/*.js");
                        }, options =>
                        {
                            options.HasInput((inputOptions) =>
                            {
                                inputOptions.AddEntryPoint("/esm/main-a.js")
                                    .AddEntryPoint("/esm/main-b.js");
                            });

                            options.HasOutput((output) =>
                            {
                                output.Format = Rollup.RollupOutputFormat.Esm;
                                output.Dir = "/rollup/module/";
                            });
                        })
                        // rollup code splitting example - for browsers that don't support native modules we use systemjs.
                        .AddRollupCodeSplittingPipe(input =>
                        {
                            input.Include("esm/**/*.js");
                        }, options =>
                        {
                            options.HasInput((inputOptions) =>
                            {
                                inputOptions.AddEntryPoint("/esm/main-a.js")
                                    .AddEntryPoint("/esm/main-b.js");
                            });

                            options.HasOutput((output) =>
                            {
                                output.Format = Rollup.RollupOutputFormat.System;
                                output.Dir = "/rollup/nomodule/";
                            });

                        })
                        // rollup code splitting example - produces multiple rollup builds (different output formats)
                        // from same set of input files sent to nodejs side once.
                        .AddRollupCodeSplittingPipe(input =>
                        {
                            input.Include("esm/**/*.js");
                        }, options =>
                        {
                            options.HasInput((inputOptions) =>
                            {
                                inputOptions.AddEntryPoint("/esm/main-a.js")
                                    .AddEntryPoint("/esm/main-b.js");
                            });

                            options.HasOutput((output) =>
                            {
                                output.Format = Rollup.RollupOutputFormat.System;
                                output.Dir = "/rollup/multi/nomodule/";
                            });

                            options.HasOutput((output) =>
                            {
                                output.Format = Rollup.RollupOutputFormat.Esm;
                                output.Dir = "/rollup/multi/module/";
                            });

                        })
                        // rollup hmr example using es6
                        .AddRollupCodeSplittingPipe(input =>
                        {
                            input.Include("hmr/es6/**/*.js");
                        }, options =>
                        {
                            // This can be used to replace the @hot import with an empty object. Could be useful if you want a mechanism to disable hmr?
                            //options.AddPlugin((a) =>
                            // {
                            //     a.RequiresNpmModule("rollup-plugin-ignore", "1.0.4")
                            //     .HasOptionsOfKind(OptionsKind.Array, (pluginOptions) =>
                            //     {
                            //         pluginOptions.Add("@hot");
                            //     })
                            //     .RunsBeforeSystemPlugins(); // this plugin needs to run before the "hypothetical" plugin, which is added by netpack as the first plugin by default and is what acts as the in-memory file system.
                            // });

                            options.HasInput((inputOptions) =>
                            {
                                inputOptions.AddEntryPoint("/hmr/es6/entry-a.js")
                                    .AddExternal("@hot");

                            });

                            options.HasOutput((output) =>
                            {
                                output.Format = Rollup.RollupOutputFormat.System;
                                output.Dir = "/rollup/hmr/es6/nomodule/";
                            });

                            options.HasOutput((output) =>
                            {
                                output.Format = Rollup.RollupOutputFormat.Esm;
                                output.Dir = "/rollup/hmr/es6/module/";
                            });

                        })
                        // add hmr exampler using AMD
                        .AddRollupPipe(input =>
                        {
                            input.Include("hmr/amd/**/*.js");
                        }, options =>
                        {
                            // we add a rollup plugin which converts AMD files to ES2016 modules, so they can be included in a rollup bundle.
                            // https://www.npmjs.com/package/rollup-plugin-amd
                            options
                                .ImportModuleLookupAmd() // imports lookup-amd into rollup script so plugins can use lookup() to lookup amd modules.   
                                .AddPluginAmd((amdPluginOptions) =>
                                {
                                    amdPluginOptions.RewireFunction(
                                        "function (moduleId, parentPath) { return lookup({ partial: moduleId, filename: parentPath, config: {baseUrl: '/hmr/amd'} }); }");
                                })
                                .HasInput((inputOptions) =>
                                {
                                    inputOptions.Input = "/hmr/amd/SomePage.js";
                                })
                                .HasOutput((output) =>
                                {
                                    output.Format = Rollup.RollupOutputFormat.Iife;
                                    //  output. = "/rollup/hmr/es6/module/";
                                    // output.File = "SomePage.js";
                                    output.File = "/rollup/hmr/amd/module/SomePage.js";
                                });
                        })
                        //.ShouldPerformRequirementsCheck(p => false) // can disable checking of requirements on startup for speedier startup, for example checking all required npm modules are installed. By default this check is enabled.

                        .UseBaseRequestPath("/netpack") // serves all outputs using the specified base request path.
                        .Watch(500); // Inputs are monitored, and when changes occur, pipes will automatically re-process, with a delay of 500ms to consolidate duplicate file change token signalling into a single trigger.

                });
            });
               
            // TODO: Add back support for this..
            // , (nodeOptions)
            //  =>
            // {
            //     // We are optionally modifying how node services are configued, so that we can
            //     // pick where any necessary npm modules will be resolved / installed to (default is directly under project dir/node_modules. 
            //     string nodeProjectDir = System.IO.Path.Combine(nodeOptions.ProjectPath, "bin/netpack-npm");
            //     nodeOptions.ProjectPath = nodeProjectDir;
            //     if (!System.IO.Directory.Exists(nodeProjectDir))
            //     {
            //         System.IO.Directory.CreateDirectory(nodeProjectDir);
            //
            //     }
            // }); 

            services.AddBrowserReload((options) =>
            {
                // trigger browser reload when our bundle file changes.
                options.FileProvider(Environment.WebRootFileProvider)
                        .Watch("/netpack/built.js");

                // trigger browser reload when any content cshtml files change.
                options.FileProvider(Environment.ContentRootFileProvider)
                        .Watch("/Views/**/*.cshtml");

               // options.SetDelay(2000); // allows multiple changes to accumulate before triggering refresh.

            });


            services.AddHotModuleReload((options) =>
            {
                // trigger hmr event when outputs change.
                options.WatchWebRoot((patterns) =>
                {
                    patterns.Include("/netpack/rollup/hmr/es6/nomodule/entry-a.js")
                            .Include("/netpack/rollup/hmr/amd/**/*.js")
                            .Include("/netpack/built.js")
                            .Include("/amd/**/*.js")
                            .Include("/hmr/amd/**/*.js");
                });
            });

            services.AddMvc();
            services.AddSignalR();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
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
            
            app.UseRouting();
            app.UseAuthorization();

          //  app.UseBrowserReload();
           //S app.UseHotModuleReload();
            // UseBrowserReload() calls UseSignalR() under the hood with default options.
            // If you want full control of singlar setup, use the following instead:
            //app.UseSignalR(routes =>
            //{
            //    routes.MapBrowserReloadHub();
            //});
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBrowserReloadHub();
                endpoints.MapHotModuleReloadHub();
                // Map default controller route
                // endpoints.MapControllerRoute(
                //     name: "default",
                //     pattern: "{controller=Home}/{action=Index}/{id?}");
                
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=SingleTypescriptFile}/{id?}");
                
            
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
