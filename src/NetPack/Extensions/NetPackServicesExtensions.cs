using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipes;
using NetPack.Requirements;


namespace NetPack.Extensions
{
    public static class NetPackServicesExtensions
    {
        public static IServiceCollection AddNetPack(this IServiceCollection services)
        {
            // Enable Node Services
            services.AddNodeServices(new NodeServicesOptions() { HostingModel = NodeHostingModel.Socket });
            services.AddSingleton(new NodeJsRequirement());
            services.AddSingleton<IRequirement>(new NodeJsRequirement());

            return services;
        }
        public static IApplicationBuilder UseNetPackPipeLine(this IApplicationBuilder appBuilder, Action<PipeLineBuilder> createPipeline)
        {
            // Enable Node Services
            var builder = new PipeLineBuilder(appBuilder);
            createPipeline(builder);

            //services.ApplicationServices.GetService()
        

            return appBuilder;
        }

    }
}