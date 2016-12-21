using System.Collections.Generic;

namespace NetPack.Typescript
{
    public class TypeScriptCompileResult
    {
        public Dictionary<string, string> Sources { get; set; }
        public TypescriptCompileError[] Errors { get; set; }
        public string Message { get; set; }
    }
}
