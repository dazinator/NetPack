using System;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipeline;
using NetPack.Requirements;
using NetPack.Utils;
using NetPack.Typescript;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public static class PipelineBuilderTypescriptExtensions
    {

        public static IPipelineBuilder AddTypeScriptPipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Action<TypeScriptPipeOptions> configureOptions = null)
        {
            var appServices = builder.ServiceProvider;
            var nodeServices = (INetPackNodeServices)appServices.GetRequiredService(typeof(INetPackNodeServices));

            // add requirements to the pipeline to check nodejs is installed, and the npm packages we need.
            builder.DependsOnNode((deps) => deps.AddDependency("typescript", "3.1.1")
                                                .AddDependency("netpack-typescript-compiler", "0.0.7"));

            var embeddedResourceProvider = (IEmbeddedResourceProvider)appServices.GetRequiredService(typeof(IEmbeddedResourceProvider));

            var tsOptions = new TypeScriptPipeOptions();
            if (configureOptions != null)
            {
                configureOptions(tsOptions);
            }

            var pipe = new TypeScriptCompilePipe(nodeServices, embeddedResourceProvider, tsOptions);

            builder.AddPipe(input, pipe);
            return builder;
        }

    }



}