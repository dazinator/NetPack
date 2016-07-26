using NetPack.File;

namespace NetPack.Pipeline
{
    public interface IPipelineContext
    {
        /// <summary>
        /// The files provided to this pipe as input, for processing.
        /// </summary>
        SourceFile[] Input { get; }

        void AddOutput(SourceFile info);

    }
}