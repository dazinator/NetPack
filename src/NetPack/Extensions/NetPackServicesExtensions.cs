using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipeline;
using NetPack.Requirements;
using NetPack.Utils;
using System;
using Dazinator.Extensions.FileProviders.InMemory.Directory;
using Jering.Javascript.NodeJS;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public static class NetPackServicesExtensions
    {
        private class PipelineSetup
        {
            public IPipeLine Pipeline { get; set; }
        }

        public class FileProcessingOptions
        {
            private readonly IServiceCollection _services;

            public FileProcessingOptions(IServiceCollection services)
            {
                _services = services;
            }

            public FileProcessingOptions AddPipeline(Action<IPipelineConfigurationBuilder> processorBuilder)
            {
                _services.AddTransient<PipelineSetup>((sp) =>
                {
                    IDirectory sourcesDirectory = sp.GetService<IDirectory>();
                    PipelineConfigurationBuilder builder = new PipelineConfigurationBuilder(sp, sourcesDirectory);

                    //var builder = new PipelineConfigurationBuilder(sp, sourcesDirectory);
                    processorBuilder(builder);
                    IPipeLine pipeline = builder.BuildPipeLine();
                    PipelineManager pipeLineManager = sp.GetService<PipelineManager>();
                    pipeLineManager.AddPipeLine(builder.Name, pipeline, builder.WatchInput, builder.WatchTriggerDelay);
                    return new PipelineSetup() { Pipeline = pipeline };
                });

                return this;
            }
        }

        public static IServiceCollection AddNetPack(this IServiceCollection services,
            Action<FileProcessingOptions> configureOptions
            //Action<NodeServicesOptions> configureGlobalNodeOptions = null
        )
        {
            services.AddSingleton(new NodeJsIsInstalledRequirement());
            services.AddSingleton<IRequirement, NpmDependenciesRequirement>();
            services.AddSingleton<NpmDependencyList>();

            services.AddSingleton<PipelineManager>();
            // services.AddSingleton<INetPackPipelineFileProvider, NetPackPipelineFileProvider>();
            services.AddSingleton<IEmbeddedResourceProvider, EmbeddedResourceProvider>();
            services.AddSingleton<IPipelineWatcher, PipelineWatcher>();
            services
                .AddTransient<IDirectory,
                    InMemoryDirectory>(); // directory used for exposing source files that need be served up when source mapping is enabled.


            if (configureOptions != null)
            {
                FileProcessingOptions opts = new FileProcessingOptions(services);
                configureOptions(opts);
            }

            services.AddNodeJS();
            services.Configure<NodeJSProcessOptions>(options =>
            {
                // Configure NodeJS to use the current directory as the project path
                // This ensures node_modules can be resolved correctly
                options.ProjectPath = Environment.CurrentDirectory;
            });
            services.AddSingleton(typeof(INetPackNodeServices), serviceProvider =>
            {
                //var nodeServices = serviceProvider.GetRequiredService<INodeServices>();
                //  NodeServicesOptions options = new NodeServicesOptions(serviceProvider); // Obtains default options from DI config
                // otherwise node services restarts automatically when file changes are made, losing state - we want to handle watch on netcore side.
                //  options.WatchFileExtensions = null;
                //  configureGlobalNodeOptions?.Invoke(options);
                var nodeServices = serviceProvider.GetRequiredService<INodeJSService>();
                return new NetPackNodeServices(nodeServices);
            });

            return services;
        }

        /// <summary>
        /// Initialises all file processing, and also adds middleware for delaying a http request for a file that is in still in the process of being generated.
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <param name="requestTimeout">The maximum amount of time a request will be delayed whilst waiting for a pipeline to process any updated inputs.
        /// For example, if you change a file, and a pipeline needs to re-process it to produce some output, a request for the output file will be delayed until the output is up to date, or this timeout is reached.
        /// If null then default of 1 minute is used.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseNetPack(this IApplicationBuilder appBuilder,
            TimeSpan? requestTimeout = null)
        {
            PipelineManager pipeLineManager = appBuilder.ApplicationServices.GetService<PipelineManager>();
            if (pipeLineManager == null)
            {
                throw new Exception(
                    "Could not find a required netpack service. Have you called services.AddNetPack() in your startup class?");
            }

            // Triggers all pipeline to be initialised and registered with pipeline manager.
            System.Collections.Generic.IEnumerable<PipelineSetup> initialisedPipelines =
                appBuilder.ApplicationServices.GetServices<PipelineSetup>();

            RequestHaltingMiddlewareOptions middlewareOptions = new RequestHaltingMiddlewareOptions();
            if (requestTimeout != null)
            {
                middlewareOptions.Timeout = requestTimeout.Value;
            }

            appBuilder.UseMiddleware<RequestHaltingMiddleware>(middlewareOptions);

            return appBuilder;

            // return new NetPackApplicationBuilder(appBuilder, pipeline);
        }
    }
}