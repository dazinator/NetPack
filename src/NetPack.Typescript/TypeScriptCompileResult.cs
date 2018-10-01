using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NetPack.Typescript
{
    public class TypeScriptCompileResult
    {
        public Dictionary<string, string> Sources { get; set; }
        public TypescriptCompileError[] Errors { get; set; }
        public string Message { get; set; }

        public JObject Echo { get; set; }

        public Dictionary<string, string> EchoFiles { get; set; }
    }
}
