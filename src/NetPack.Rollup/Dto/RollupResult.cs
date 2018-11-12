using Newtonsoft.Json.Linq;

namespace NetPack.Rollup
{
    public class RollupResult
    {
        public JValue Code { get; set; }

        public SourceMap SourceMap { get; set; }

        public string FileName { get; set; }

        public string[] Exports { get; set; }

        public string[] Imports { get; set; }

        public bool IsEntry { get; set; }

        public RollupModuleResult[] Modules { get; set; }
    }
}