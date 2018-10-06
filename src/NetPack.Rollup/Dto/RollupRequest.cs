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

        public RollupPipeOptions Options { get; set; }
    
        public List<NodeInMemoryFile> Files { get; set; }   
    }
}