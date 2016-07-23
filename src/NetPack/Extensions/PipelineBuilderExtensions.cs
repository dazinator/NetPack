using System;
using System.IO;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipes;
using NetPack.Requirements;
using NetPack.Tests;

namespace NetPack.Extensions
{

    public class TypeScriptPipeOptions
    {

    }


    public static class PipelineBuilderExtensions
    {

        public static PipeLineBuilder AddTypeScriptPipe(this PipeLineBuilder builder, Action<TypeScriptPipeOptions> configureOptions)
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