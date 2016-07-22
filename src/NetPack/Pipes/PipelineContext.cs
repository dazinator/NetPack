using System.Collections.Generic;

namespace NetPack.Pipes
{
    public class PipelineContext : IPipelineContext
    {

        public PipelineContext()
        {
            OutputFiles = new List<SourceFile>();
            InputFiles = new List<SourceFile>();
        }

        public SourceFile[] Input => InputFiles.ToArray();

        public void AddOutput(SourceFile info)
        {
            OutputFiles.Add(info);
        }

        public List<SourceFile> OutputFiles { get; set; }

        public List<SourceFile> InputFiles { get; set; }
    }
}