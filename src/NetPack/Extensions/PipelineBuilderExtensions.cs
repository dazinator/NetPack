using System;
using System.IO;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipeline;
using NetPack.Pipes;
using NetPack.Requirements;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public static class PipelineBuilderExtensions
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

            var typescriptSimplePackageRequirement = new NpmModuleRequirement("typescript-simple", true);
            builder.IncludeRequirement(typescriptSimplePackageRequirement);

            //var nodeJsRequirement = (NodeJsRequirement)appServices.GetRequiredService(typeof(NodeJsRequirement));

            //  var nodeServices = builder.ApplicationBuilder.ApplicationServices.GetService(typeof(INodeServices), nodeJsRequirement)

            var tsOptions = new TypeScriptPipeOptions();
            configureOptions(tsOptions);
            var pipe = new TypeScriptCompilePipe(nodeServices, nodeJsRequirement, tsOptions);

            builder.AddPipe(pipe);
            return builder;
        }

    }


}