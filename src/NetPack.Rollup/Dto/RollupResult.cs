using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace NetPack.Rollup
{
    public class RollupResult
    {
        [JsonPropertyName("Code")]
        public JsonValue Code { get; set; }

        [JsonPropertyName("SourceMap")]
        public SourceMap SourceMap { get; set; }

        [JsonPropertyName("FileName")]
        public string FileName { get; set; }

        [JsonPropertyName("Exports")]
        public string[] Exports { get; set; }

        [JsonPropertyName("Imports")]
        public string[] Imports { get; set; }

        [JsonPropertyName("IsEntry")]
        public bool IsEntry { get; set; }

        [JsonPropertyName("Modules")]
        public RollupModuleResult[] Modules { get; set; }

        [JsonPropertyName("Id")]
        public string Id { get; set; }
    }
}