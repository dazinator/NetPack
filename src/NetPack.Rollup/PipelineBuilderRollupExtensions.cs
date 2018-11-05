using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetPack.Pipeline;
using NetPack.Requirements;
using NetPack.Rollup;
using NetPack.Utils;
using System;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{

    public static class PipelineBuilderRollupExtensions
    {

        public static IPipelineBuilder AddRollupPipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Action<RollupPipeOptionsBuilder> configureOptions = null)
        {

            var nodeServices = AddRollup(builder);

            RollupPipeOptionsBuilder optionsBuilder = new RollupPipeOptionsBuilder(builder);
            configureOptions?.Invoke(optionsBuilder);

            IServiceProvider appServices = builder.ServiceProvider;
            IEmbeddedResourceProvider embeddedResourceProvider = (IEmbeddedResourceProvider)appServices.GetRequiredService(typeof(IEmbeddedResourceProvider));

            ILogger<RollupPipe> logger = (ILogger<RollupPipe>)appServices.GetRequiredService(typeof(ILogger<RollupPipe>));
            RollupPipe pipe = new RollupPipe(nodeServices, embeddedResourceProvider, logger, optionsBuilder.InputOptions, optionsBuilder.OutputOptions);
            builder.AddPipe(input, pipe);
            return builder;
        }

        private static INetPackNodeServices AddRollup(IPipelineBuilder builder)
        {
            IServiceProvider appServices = builder.ServiceProvider;
            INetPackNodeServices nodeServices = (INetPackNodeServices)appServices.GetRequiredService(typeof(INetPackNodeServices));

            // add requirements to the pipeline to check nodejs is installed, and the npm packages we need.
            NodeJsRequirement nodeJsRequirement = new NodeJsRequirement();
            builder.IncludeRequirement(nodeJsRequirement);

            NpmModuleRequirement rollupModule = new NpmModuleRequirement("rollup", true, "0.66.2");
            builder.IncludeRequirement(rollupModule);

            NpmModuleRequirement rollupPluginHypothetical = new NpmModuleRequirement("rollup-plugin-hypothetical", true, "2.1.0");
            builder.IncludeRequirement(rollupPluginHypothetical);

            NpmModuleRequirement netpackRollup = new NpmModuleRequirement("netpack-rollup", true, "1.0.16");
            builder.IncludeRequirement(netpackRollup);

            return nodeServices;
        }

        public static IPipelineBuilder AddRollupCodeSplittingPipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Action<RollupPipeCodeSplittingOptionsBuilder> configureOptions = null)
        {

            var nodeServices = AddRollup(builder);

            RollupPipeCodeSplittingOptionsBuilder optionsBuilder = new RollupPipeCodeSplittingOptionsBuilder(builder);
            configureOptions?.Invoke(optionsBuilder);

            IServiceProvider appServices = builder.ServiceProvider;
            IEmbeddedResourceProvider embeddedResourceProvider = (IEmbeddedResourceProvider)appServices.GetRequiredService(typeof(IEmbeddedResourceProvider));

            ILogger<RollupCodeSplittingPipe> logger = (ILogger<RollupCodeSplittingPipe>)appServices.GetRequiredService(typeof(ILogger<RollupCodeSplittingPipe>));
            RollupCodeSplittingPipe pipe = new RollupCodeSplittingPipe(nodeServices, embeddedResourceProvider, logger, optionsBuilder.InputOptions, optionsBuilder.OutputOptions);
            builder.AddPipe(input, pipe);
            return builder;
        }

    }
}