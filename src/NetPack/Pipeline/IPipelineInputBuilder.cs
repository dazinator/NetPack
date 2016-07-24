namespace NetPack.Pipeline
{
    public interface IPipelineInputBuilder
    {
        IPipelineInputBuilder WatchInputForChanges();


        // flushes the input through the pipeline. The input will be processed according to the pipeline, and outputs
        // will be returned.  
        IPipeLine BuildPipeLine();

    }
}