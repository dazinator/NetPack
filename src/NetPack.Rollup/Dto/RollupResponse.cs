using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NetPack.Rollup
{
    public class RollupResponse
    {
        public Dictionary<string, RollupResult[]> Results { get; set; }

        public JObject Echo { get; set; }

        public JArray EchoArray { get; set; }

        public JValue EchoValue { get; set; }
    }
}