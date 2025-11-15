using System.Text.Json.Nodes;

namespace NetPack.Rollup
{
    public class SourceMap
    {
        public int Version { get; set; }
        public string File { get; set; }
        public string[] Sources { get; set; }
        public string[] SourcesContent { get; set; }
        public string[] Names { get; set; }
        public JsonValue Mappings { get; set; }       

    }
}