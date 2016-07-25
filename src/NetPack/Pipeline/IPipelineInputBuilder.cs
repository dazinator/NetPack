using Microsoft.AspNetCore.Builder;
using NetPack.Pipes;
using NetPack.Requirements;

namespace NetPack.Pipeline
{
    public interface IPipelineInputOptionsBuilder
    {
        IPipelineInputOptionsBuilder WatchInputForChanges();

        IPipelineBuilder DefinePipeline();

    }

    public interface IPipelineBuilder
    {

        IApplicationBuilder ApplicationBuilder { get; set; }

        IPipelineBuilder AddPipe(IPipe pipe);

        // flushes the input through the pipeline. The input will be processed according to the pipeline, and outputs
        // will be returned.  
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