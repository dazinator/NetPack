
using System;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Requirements;
using NetPack.Utils;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.Extensions.Logging;

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

            public FileProcessingOptions AddFileProcessing(Action<IPipelineConfigurationBuilder> processorBuilder)
            {
                _services.AddTransient<PipelineSetup>((sp) =>
                {

                    var sourcesDirectory = sp.GetService<IDirectory>();
                    var builder = new PipelineConfigurationBuilder(sp, sourcesDirectory);

                    //var builder = new PipelineConfigurationBuilder(sp, sourcesDirectory);
                    processorBuilder(builder);
                    var pipeline = builder.BuildPipeLine();
                    var pipeLineManager = sp.GetService<PipelineManager>();
                    pipeLineManager.AddPipeLine(builder.Name, pipeline, builder.WachInput);
                    return new PipelineSetup() { Pipeline = pipeline };
                });

                return this;
            }

        }

        public static IServiceCollection AddNetPack(this IServiceCollection services, Action<FileProcessingOptions> configureOptions)
        {
            // Enable Node Services
            services.AddNodeServices((options) =>
            {
                options.HostingModel = NodeHostingModel.Socket;
            });


            services.AddSingleton(typeof(INetPackNodeServices), serviceProvider =>
            {
                var options = new NodeServicesOptions(serviceProvider); // Obtains default options from DI config

                var nodeServices = NodeServicesFactory.CreateNodeServices(options);
                return new NetPackNodeServices(nodeServices);
            });

            services.AddSingleton(new NodeJsRequirement());
            services.AddSingleton<IRequirement>(new NodeJsRequirement());
            services.AddSingleton<PipelineManager>();
            // services.AddSingleton<INetPackPipelineFileProvider, NetPackPipelineFileProvider>();
            services.AddSingleton<IEmbeddedResourceProvider, EmbeddedResourceProvider>();
            services.AddSingleton<IPipelineWatcher, PipelineWatcher>();
            services.AddTransient<IDirectory, InMemoryDirectory>(); // directory used for exposing source files that need be served up when source mapping is enabled.

            if (configureOptions != null)
            {
                var opts = new FileProcessingOptions(services);
                configureOptions(opts);
            }

            return services;
        }

        /// <summary>
        /// Causes all file processing pipelines to be initialised, and adds middleware for delaying a http request for a generated file that is in the process of being re-generated.
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseFileProcessing(this IApplicationBuilder appBuilder)
        {
            var pipeLineManager = appBuilder.ApplicationServices.GetService<PipelineManager>();
            if (pipeLineManager == null)
            {
                throw new Exception(
                    "Could not find a required netpack service. Have you called services.AddNetPack() in your startup class?");
            }

            // Triggers all pipeline to be initialised and registered with pipeline manager.
            var initialisedPipelines = appBuilder.ApplicationServices.GetServices<PipelineSetup>();

            appBuilder.UseMiddleware<RequestHaltingMiddleware>();

            return appBuilder;

            // return new NetPackApplicationBuilder(appBuilder, pipeline);
        }
    }

 
}