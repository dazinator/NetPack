using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NetPack.Extensions;
using NetPack.Pipeline;
using NetPack.Requirements;

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
            services.AddSingleton<INetPackPipelineFileProvider, NetPackPipelineFileProvider>();

            return services;
        }

        public static INetPackApplicationBuilder UseNetPackPipeLine(this IApplicationBuilder appBuilder,
            Func<IPipelineBuilder, IPipeLine> createPipeline)
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


            var builder = new PipeLineBuilder(appBuilder);
            var pipeLine = createPipeline(builder);
            pipeLineManager.AddPipeLine(pipeLine);

            //   var hostingEnv = appBuilder.ApplicationServices.GetService<IHostingEnvironment>();
            //  var existingStaticFilesProvider = staticFilesOptions.Value.FileProvider ?? hostingEnv.WebRootFileProvider;
            //  appBuilder.

            return new NetPackApplicationBuilder(appBuilder, pipeLine);
        }

        public static IApplicationBuilder UseNetPackStaticFiles(this INetPackApplicationBuilder appBuilder, string servePath = "")
        {

            // NetPackPipelineFileProvider
            var fileProvider = new NetPackPipelineFileProvider(appBuilder.PipeLine);
            appBuilder.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = fileProvider,
                RequestPath = string.IsNullOrWhiteSpace(servePath) ? null : new PathString(servePath)
            });

            return appBuilder;
        }


    }
}