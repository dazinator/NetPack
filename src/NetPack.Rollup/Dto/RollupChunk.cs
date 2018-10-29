using Newtonsoft.Json.Linq;

namespace NetPack.Rollup
{
    public class RollupChunk
    {
        public JValue Code { get; set; }

        public JValue SourceMap { get; set; }

        public string FileName { get; set; }

    }
}