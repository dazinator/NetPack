using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
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

            return services;
        }
        public static IApplicationBuilder UseNetPackPipeLine(this IApplicationBuilder appBuilder, Func<IPipelineBuilder, IPipeLine> createPipeline)
        {
            var pipeLineManager = appBuilder.ApplicationServices.GetService<PipelineManager>();
            if (pipeLineManager == null)
            {
                throw new Exception("Could not find a required netpack service. Have you called services.AddNetPack() in your startup class?");
            }

            var builder = new PipeLineBuilder(appBuilder);
            var pipeLine = createPipeline(builder);
            pipeLineManager.AddPipeLine(pipeLine);

            return appBuilder;
        }

    }
}