using System.Collections.Generic;
using NetPack.File;

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