using System;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipeline;
using NetPack.Pipes;
using NetPack.Pipes.Typescript;
using NetPack.Requirements;
using NetPack.Utils;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public static class PipelineBuilderTypescriptExtensions
    {

        public static IPipelineBuilder AddTypeScriptPipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Action<TypeScriptPipeOptions> configureOptions = null)
        {
            var appServices = builder.ApplicationBuilder.ApplicationServices;
            var nodeServices = (INodeServices)appServices.GetRequiredService(typeof(INodeServices));

            // add requirements to the pipeline to check nodejs is installed, and the npm packages we need.
            var nodeJsRequirement = new NodeJsRequirement();
            builder.IncludeRequirement(nodeJsRequirement);

            var typescriptPackageRequriement = new NpmModuleRequirement("typescript", true);
            builder.IncludeRequirement(typescriptPackageRequriement);

            var typescriptSimplePackageRequirement = new NpmModuleRequirement("netpack-typescript-compiler", true, "0.0.4");
            builder.IncludeRequirement(typescriptSimplePackageRequirement);

            //var nodeJsRequirement = (NodeJsRequirement)appServices.GetRequiredService(typeof(NodeJsRequirement));

            //  var nodeServices = builder.ApplicationBuilder.ApplicationServices.GetService(typeof(INodeServices), nodeJsRequirement)

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

    public static class PipelineBuilderCombineExtensions
    {

        public static IPipelineBuilder AddJsCombinePipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Func<string> outputFilePath, Action<JsCombinePipeOptions> configureOptions = null)
        {
            var options = new JsCombinePipeOptions();
            var outputfile = outputFilePath();
            if (string.IsNullOrEmpty(outputfile))
            {
                throw new ArgumentNullException(nameof(outputfile));
            }
            options.OutputFilePath = outputfile;
            if (configureOptions != null)
            {
                configureOptions(options);
            }
            var pipe = new JsCombinePipe(options);

            builder.AddPipe((inputBuilder) =>
            {
                // automatically exclude the output / combined file from pipe input.
                inputBuilder.Exclude(outputfile);
                input(inputBuilder);
            }, pipe);

            builder.AddPipe(input, pipe);
            return builder;
        }

    }

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