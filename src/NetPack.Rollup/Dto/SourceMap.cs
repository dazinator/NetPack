using Newtonsoft.Json.Linq;

namespace NetPack.Rollup
{
    public class SourceMap
    {
        public string Version { get; set; }
        public string File { get; set; }
        public string[] Sources { get; set; }
        public string[] SourcesContent { get; set; }
        public string[] Names { get; set; }
        public JValue Mappings { get; set; }

    }
}