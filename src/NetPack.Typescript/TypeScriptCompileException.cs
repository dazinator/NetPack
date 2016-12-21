using System;
using System.Collections.Generic;

namespace NetPack.Typescript
{
    public class TypeScriptCompileException : Exception
    {
        public TypeScriptCompileException() : base()
        {
            Errors = new List<TypescriptCompileError>();
        }
        public TypeScriptCompileException(string message, IEnumerable<TypescriptCompileError> errors) : base(message)
        {
            Errors = new List<TypescriptCompileError>();
            Errors.AddRange(errors);
        }

        public List<TypescriptCompileError> Errors { get; set; }

    }
}
