using NetPack.Node.Dto;
using System.Collections.Generic;

namespace NetPack.Rollup
{
    public class RollupRequest
    {
        public RollupRequest()
        {
            Files = new List<NodeInMemoryFile>();
        }

        public RollupInputOptions InputOptions { get; set; }

        public RollupOutputFileOptions OutputOptions { get; set; }        

        public List<NodeInMemoryFile> Files { get; set; }   
    }
}