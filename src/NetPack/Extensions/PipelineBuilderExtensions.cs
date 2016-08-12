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

        public static IPipelineBuilder AddTypeScriptPipe(this IPipelineBuilder builder, Action<TypeScriptPipeOptions> configureOptions)
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
            configureOptions(tsOptions);
            var pipe = new TypeScriptCompilePipe(nodeServices, embeddedResourceProvider, tsOptions);

            builder.AddPipe(pipe);
            return builder;
        }

    }

    public static class PipelineBuilderCombineExtensions
    {

        public static IPipelineBuilder AddCombinePipe(this IPipelineBuilder builder, Action<CombinePipeOptions> configureOptions)
        {
            var options = new CombinePipeOptions();
            configureOptions(options);
            var pipe = new CombinePipe(options);
            builder.AddPipe(pipe);
            return builder;
        }

    }


}