using Microsoft.AspNetCore.Builder;
using NetPack.Pipes;

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
    }
}