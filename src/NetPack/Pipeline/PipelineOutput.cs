using System.Collections.Generic;

namespace NetPack.Pipeline
{
    public class PipelineOutput
    {
        public PipelineOutput(List<SourceFile> files)
        {
            Files = files;
        }

        public List<SourceFile> Files { get; }


    }
}