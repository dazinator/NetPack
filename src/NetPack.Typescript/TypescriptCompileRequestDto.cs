using System.Collections.Generic;

namespace NetPack.Typescript
{
    public class TypescriptCompileRequestDto
    {
        public TypescriptCompileRequestDto()
        {
            Files = new Dictionary<string, string>();
            Options = new TypeScriptPipeOptions();
            Inputs = new List<string>();
        }
        public Dictionary<string, string> Files { get; set; }
        public List<string> Inputs { get; set; }
        public TypeScriptPipeOptions Options { get; set; }

        public Dictionary<string, string> EchoFiles { get; set; }

    }
}
