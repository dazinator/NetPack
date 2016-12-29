using System;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipeline;
using NetPack.RequireJs;
using NetPack.Requirements;
using NetPack.Utils;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    
    public static class PipelineBuilderRequireJsOptimiseExtensions
    {

        public static IPipelineBuilder AddRequireJsOptimisePipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Action<RequireJsOptimisationPipeOptions> configureOptions = null)
        {

            var appServices = builder.ServiceProvider;
            var nodeServices = (INodeServices)appServices.GetRequiredService(typeof(INodeServices));

            // add requirements to the pipeline to check nodejs is installed, and the npm packages we need.
            var nodeJsRequirement = new NodeJsRequirement();
            builder.IncludeRequirement(nodeJsRequirement);

            var netpackRequireJsRequirement = new NpmModuleRequirement("netpack-requirejs", true, "0.0.2");
            builder.IncludeRequirement(netpackRequireJsRequirement);

            var options = new RequireJsOptimisationPipeOptions();
            if (configureOptions != null)
            {
                configureOptions(options);
            }

            var embeddedResourceProvider = (IEmbeddedResourceProvider)appServices.GetRequiredService(typeof(IEmbeddedResourceProvider));
            var logger = (ILogger<RequireJsOptimisePipe>)appServices.GetRequiredService(typeof(ILogger<RequireJsOptimisePipe>));
            var pipe = new RequireJsOptimisePipe(nodeServices, embeddedResourceProvider, logger,options);
            builder.AddPipe(input, pipe);
            return builder;
        }

    }
}