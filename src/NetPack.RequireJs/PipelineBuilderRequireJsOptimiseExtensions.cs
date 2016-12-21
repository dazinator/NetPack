using System;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipeline;
using NetPack.Pipes;
using NetPack.Requirements;
using NetPack.Utils;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    
    public static class PipelineBuilderRequireJsOptimiseExtensions
    {

        public static IPipelineBuilder AddRequireJsOptimisePipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Action<RequireJsOptimisationPipeOptions> configureOptions = null)
        {

            var appServices = builder.ApplicationBuilder.ApplicationServices;
            var nodeServices = (INodeServices)appServices.GetRequiredService(typeof(INodeServices));

            // add requirements to the pipeline to check nodejs is installed, and the npm packages we need.
            var nodeJsRequirement = new NodeJsRequirement();
            builder.IncludeRequirement(nodeJsRequirement);

            var netpackRequireJsRequirement = new NpmModuleRequirement("netpack-requirejs", true);
            builder.IncludeRequirement(netpackRequireJsRequirement);

            var options = new RequireJsOptimisationPipeOptions();
            if (configureOptions != null)
            {
                configureOptions(options);
            }

            var embeddedResourceProvider = (IEmbeddedResourceProvider)appServices.GetRequiredService(typeof(IEmbeddedResourceProvider));

            var pipe = new RequireJsOptimisePipe(nodeServices, embeddedResourceProvider, options);
            builder.AddPipe(input, pipe);
            return builder;
        }

    }
}