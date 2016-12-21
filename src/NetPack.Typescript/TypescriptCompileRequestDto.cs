using System.Collections.Generic;

namespace NetPack.Typescript
{
    public class TypescriptCompileRequestDto
    {
        public TypescriptCompileRequestDto()
        {
            Files = new Dictionary<string, string>();
            Options = new TypeScriptPipeOptions();
        }
        public Dictionary<string, string> Files { get; set; }
        public TypeScriptPipeOptions Options { get; set; }

    }
}
