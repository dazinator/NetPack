using System.Text.Json.Serialization;

namespace NetPack.Node.Dto
{

    public class NodeInMemoryFile
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }
        
        [JsonPropertyName("contents")]
        public string Contents { get; set; }
    }
}