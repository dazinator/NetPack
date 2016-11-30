using Microsoft.AspNetCore.Builder;
using NetPack.Pipes;
using NetPack.Requirements;
using System;

namespace NetPack.Pipeline
{
    public interface IPipelineInputOptionsBuilder
    {
       // IPipelineInputOptionsBuilder Watch();

        ///// <summary>
        ///// The path to the WebRoot folder within the Content directory.
        ///// </summary>
        ///// <param name="webrootPath"></param>
        ///// <returns></returns>
        //IPipelineInputOptionsBuilder FromWebRoot(string webrootPath);


       // IPipelineBuilder BeginPipeline();

    }

    public interface IPipelineBuilder
    {

        IApplicationBuilder ApplicationBuilder { get; set; }

        IPipelineBuilder Watch();

        IPipelineBuilder AddPipe(Action<PipelineInputBuilder> inputBuilder, IPipe pipe);
       
        
        IPipeLine BuildPipeLine();

        /// <summary>
        /// Adds a requirement to this pipeline. When the pipeline is initialised, all requirements are checked.
        /// This can be used to ensure that dependent services are installed etc, like node.js, or npm modules.
        /// </summary>
        /// <param name="requiement"></param>
        /// <returns></returns>
        IPipelineBuilder IncludeRequirement(IRequirement requiement);
    }
}