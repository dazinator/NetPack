using NetPack.Node.Dto;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetPack.Rollup
{
    public class RollupRequest
    {
        public RollupRequest()
        {
            Files = new List<NodeInMemoryFile>();
        }

        [JsonPropertyName("inputOptions")]
        public BaseRollupInputOptions InputOptions { get; set; }

        [JsonPropertyName("outputOptions")]
        public BaseRollupOutputOptions[] OutputOptions { get; set; }        

        [JsonPropertyName("files")]
        public List<NodeInMemoryFile> Files { get; set; }   
    }
}