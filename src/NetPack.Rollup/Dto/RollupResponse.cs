using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace NetPack.Rollup
{
    public class RollupResponse
    {
        public Dictionary<string, RollupResult[]> Results { get; set; }

        public JsonObject Echo { get; set; }

        public JsonArray EchoArray { get; set; }

        public JsonValue EchoValue { get; set; }
    }
}