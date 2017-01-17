
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
using Dazinator.AspNet.Extensions.FileProviders.Directory;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public static class NetPackServicesExtensions
    {
        public static IServiceCollection AddNetPack(this IServiceCollection services)
        {
            // Enable Node Services
            services.AddNodeServices((options) =>
            {
                options.
                HostingModel = NodeHostingModel.Socket;
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
            services.AddTransient<IPipelineConfigurationBuilder>((sp) =>
            {


                var sourcesDirectory = sp.GetService<IDirectory>();
                var builder = new PipelineConfigurationBuilder(sp, sourcesDirectory);
                return builder;

                //return new NetPackApplicationBuilder(appBuilder, pipeline);
            });



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

            var services = appBuilder.ApplicationServices;
            var sourcesDirectory = services.GetService<IDirectory>();
            var builder = new PipelineConfigurationBuilder(services, sourcesDirectory);
            processorBuilder(builder);

            var pipeline = builder.BuildPipeLine();
            pipeLineManager.AddPipeLine(builder.Name, pipeline, builder.WachInput);

            return new NetPackApplicationBuilder(appBuilder, pipeline);
        }
    }
}