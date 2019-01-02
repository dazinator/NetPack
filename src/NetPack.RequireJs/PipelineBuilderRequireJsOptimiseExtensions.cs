using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetPack.Pipeline;
using NetPack.RequireJs;
using NetPack.Requirements;
using NetPack.Utils;
using System;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{

    public static class PipelineBuilderRequireJsOptimiseExtensions
    {

        public static IPipelineBuilder AddRequireJsOptimisePipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Action<RequireJsOptimisationPipeOptions> configureOptions = null)
        {

            IServiceProvider appServices = builder.ServiceProvider;
            INetPackNodeServices nodeServices = (INetPackNodeServices)appServices.GetRequiredService(typeof(INetPackNodeServices));

            // add requirements to the pipeline to check nodejs is installed, and the npm packages we need.
            builder.DependsOnNode((deps) => deps.AddDependency("netpack-requirejs", "0.0.2"));

            RequireJsOptimisationPipeOptions options = new RequireJsOptimisationPipeOptions();
            if (configureOptions != null)
            {
                configureOptions(options);
            }

            IEmbeddedResourceProvider embeddedResourceProvider = (IEmbeddedResourceProvider)appServices.GetRequiredService(typeof(IEmbeddedResourceProvider));
            ILogger<RequireJsOptimisePipe> logger = (ILogger<RequireJsOptimisePipe>)appServices.GetRequiredService(typeof(ILogger<RequireJsOptimisePipe>));
            RequireJsOptimisePipe pipe = new RequireJsOptimisePipe(nodeServices, embeddedResourceProvider, logger, options);
            builder.AddPipe(input, pipe);
            return builder;
        }

    }
}