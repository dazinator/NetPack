using System;
using System.IO;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipeline;
using NetPack.Pipes;
using NetPack.Requirements;

namespace NetPack.Extensions
{
    public static class PipelineBuilderExtensions
    {

        public static IPipelineBuilder AddTypeScriptPipe(this IPipelineBuilder builder, Action<TypeScriptPipeOptions> configureOptions)
        {
            var appServices = builder.ApplicationBuilder.ApplicationServices;
            var nodeServices = (INodeServices)appServices.GetRequiredService(typeof(INodeServices));
            var nodeJsRequirement = (NodeJsRequirement)appServices.GetRequiredService(typeof(NodeJsRequirement));

            //  var nodeServices = builder.ApplicationBuilder.ApplicationServices.GetService(typeof(INodeServices), nodeJsRequirement)

            var tsOptions = new TypeScriptPipeOptions();
            configureOptions(tsOptions);
            var pipe = new TypeScriptCompilePipe(nodeServices, nodeJsRequirement, tsOptions);

            builder.AddPipe(pipe);
            return builder;
        }

    }


}