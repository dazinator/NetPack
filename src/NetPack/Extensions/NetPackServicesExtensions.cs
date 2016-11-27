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
            //var staticFilesOptions = appBuilder.ApplicationServices.GetService<IOptions<StaticFileOptions>>();
            //if (staticFilesOptions == null)
            //{
            //    throw new Exception(
            //        "StaticFiles not detected in the pipeline. Please call app.UseStaticFiles() before calling UseNetPackPipeLine()");
            //}

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

            //   var hostingEnv = appBuilder.ApplicationServices.GetService<IHostingEnvironment>();
            //  var existingStaticFilesProvider = staticFilesOptions.Value.FileProvider ?? hostingEnv.WebRootFileProvider;
            //  appBuilder.
            //  var pipelineFileProvider = new NetPackPipelineFileProvider(pipeLine);
            return new NetPackApplicationBuilder(appBuilder, pipeline, pipelineFileProvider);
        }

        public static INetPackApplicationBuilder UseOutputAsStaticFiles(this INetPackApplicationBuilder appBuilder, string servePath = "/netpack")
        {

            // NetPackPipelineFileProvider
            var fileProvider = appBuilder.PipelineFileProvider;
            if (!string.IsNullOrWhiteSpace(servePath))
            {
                if (!servePath.StartsWith("/"))
                {
                    servePath = "/" + servePath;
                }

                fileProvider = new RequestPathFileProvider(servePath, fileProvider);

            }

            var hostingEnv = appBuilder.ApplicationServices.GetService<IHostingEnvironment>();
            if (hostingEnv.WebRootFileProvider == null)
            {
                hostingEnv.WebRootFileProvider = fileProvider;
            }
            else
            {
                var composite = new CompositeFileProvider(hostingEnv.WebRootFileProvider, fileProvider);
                hostingEnv.WebRootFileProvider = composite;
            }


            //PathString requestPath = string.IsNullOrWhiteSpace(servePath) ? null : new PathString(servePath);

            //var staticFileOptions = new StaticFileOptions()
            //{
            //    FileProvider = fileProvider,
            //    RequestPath = requestPath
            //};

            appBuilder.Pipeline.RequestPath = servePath;

            // appBuilder.UseStaticFiles(staticFileOptions);

            appBuilder.Pipeline.Initialise();

            //if (builder.WachInput)
            //{
            //    pipeLineWatcher.WatchPipeline(pipeLine);
            //}

            return appBuilder;
        }


    }
}