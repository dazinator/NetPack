using NetPack.Node.Dto;
using System.Collections.Generic;

namespace NetPack.Rollup
{
    public class RollupCodeSplittingRequest
    {
        public RollupCodeSplittingRequest()
        {
            Files = new List<NodeInMemoryFile>();
        }

        public RollupCodeSplittingInputOptions InputOptions { get; set; }

        public RollupOutputOptions OutputOptions { get; set; }        

        public List<NodeInMemoryFile> Files { get; set; }   
    }
}