using System;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Requirements;
using NetPack.Utils;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public static class NetPackServicesExtensions
    {
        public static IServiceCollection AddNetPack(this IServiceCollection services)
        {
            // Enable Node Services
            services.AddNodeServices(new NodeServicesOptions() { HostingModel = NodeHostingModel.Socket });

            services.AddSingleton(new NodeJsRequirement());
            services.AddSingleton<IRequirement>(new NodeJsRequirement());
            services.AddSingleton<PipelineManager>();
            // services.AddSingleton<INetPackPipelineFileProvider, NetPackPipelineFileProvider>();
            services.AddSingleton<IEmbeddedResourceProvider, EmbeddedResourceProvider>();
            services.AddSingleton<IPipelineWatcher, PipelineWatcher>();

            return services;
        }

        public static INetPackApplicationBuilder UseFileProcessing(this IApplicationBuilder appBuilder,
            Action<IPipelineConfigurationBuilder> processorBuilder)
        {
            var pipeLineManager = appBuilder.ApplicationServices.GetService<PipelineManager>();
            if (pipeLineManager == null)
            {
                throw new Exception(
                    "Could not find a required netpack service. Have you called services.AddNetPack() in your startup class?");
            }

            var builder = new PipelineConfigurationBuilder(appBuilder);
            processorBuilder(builder);

            var pipeline = builder.BuildPipeLine();
            pipeLineManager.AddPipeLine(pipeline);

            var pipeLineWatcher = appBuilder.ApplicationServices.GetService<IPipelineWatcher>();
            if (builder.WachInput)
            {
                pipeLineWatcher.WatchPipeline(pipeline);
            }

            var outputFileProvider = pipeline.OutputFileProvider;
            if (!string.IsNullOrWhiteSpace(pipeline.BaseRequestPath))
            {
                outputFileProvider = new RequestPathFileProvider(pipeline.BaseRequestPath, outputFileProvider);
            }

            var hostingEnv = appBuilder.ApplicationServices.GetService<IHostingEnvironment>();
            if (hostingEnv.WebRootFileProvider == null || hostingEnv.WebRootFileProvider is NullFileProvider)
            {
                hostingEnv.WebRootFileProvider = outputFileProvider;
            }
            else
            {
                var composite = new CompositeFileProvider(hostingEnv.WebRootFileProvider, outputFileProvider);
                hostingEnv.WebRootFileProvider = composite;
            }

            pipeline.Initialise();
          
            return new NetPackApplicationBuilder(appBuilder, pipeline);
        }
    }
}