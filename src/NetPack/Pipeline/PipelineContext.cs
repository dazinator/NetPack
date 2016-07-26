using System.Collections.Generic;
using NetPack.File;

namespace NetPack.Pipeline
{
    public class PipelineContext : IPipelineContext
    {

        public PipelineContext() : this(new List<SourceFile>())
        {
        }


        public PipelineContext(List<SourceFile> inputFiles)
        {
            OutputFiles = new List<SourceFile>();
            InputFiles = inputFiles;
        }

        public SourceFile[] Input => InputFiles.ToArray();

        public void AddOutput(SourceFile info)
        {
            OutputFiles.Add(info);
        }

        public List<SourceFile> OutputFiles { get; set; }

        public List<SourceFile> InputFiles { get; set; }


        public void PrepareNextInputs()
        {
            // Sets the inputs ready for the next pipe,
            // by grabbing them from the outputs of previous pipe.
            InputFiles = OutputFiles;
            OutputFiles = new List<SourceFile>();
        }
    }
}