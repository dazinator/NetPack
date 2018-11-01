using Newtonsoft.Json.Linq;

namespace NetPack.Rollup
{
    public class RollupResponse
    {
        public RollupResult Result { get; set; }

        public JObject Echo { get; set; }
    }
}