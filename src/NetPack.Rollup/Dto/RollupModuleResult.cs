using System.Text.Json.Serialization;

namespace NetPack.Rollup
{
    public class RollupModuleResult
    {
        [JsonPropertyName("OriginalLength")]
        public int OriginalLength { get; set; }

        [JsonPropertyName("RemovedExports")]
        public string[] RemovedExports { get; set; }

        [JsonPropertyName("Length")]
        public int Length { get; set; }

        [JsonPropertyName("Exports")]
        public string[] Exports { get; set; }

        [JsonPropertyName("Id")]
        public string Id { get; set; }
    }
}