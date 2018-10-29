using Newtonsoft.Json.Linq;

namespace NetPack.Rollup
{
    public class RollupCodeSplittingResponse
    {   
        public JObject Echo { get; set; }

        public RollupChunk[] Results { get; set; }

    }
}