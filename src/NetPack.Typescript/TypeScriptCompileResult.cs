using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace NetPack.Typescript
{
    public class TypeScriptCompileResult
    {
        public Dictionary<string, string> Sources { get; set; }
        public TypescriptCompileError[] Errors { get; set; }
        public string Message { get; set; }

        public JsonObject Echo { get; set; }

        public Dictionary<string, string> EchoFiles { get; set; }
    }
}
