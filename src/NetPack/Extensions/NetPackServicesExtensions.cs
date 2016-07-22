using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
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
    }
}