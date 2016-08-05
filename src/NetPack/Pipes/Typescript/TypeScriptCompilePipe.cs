using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Utils;

namespace NetPack.Pipes.Typescript
{
    public class TypeScriptCompilePipe : IPipe
    {
        private INodeServices _nodeServices;
        private TypeScriptPipeOptions _options;
        private IEmbeddedResourceProvider _embeddedResourceProvider;

        public TypeScriptCompilePipe(INodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider) : this(nodeServices, embeddedResourceProvider, new TypeScriptPipeOptions())
        {

        }

        public TypeScriptCompilePipe(INodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, TypeScriptPipeOptions options)
        {
            _nodeServices = nodeServices;
            _embeddedResourceProvider = embeddedResourceProvider;
            _options = options;
        }

        public async Task ProcessAsync(IPipelineContext context, CancellationToken cancelationToken)
        {
            var requestDto = new TypescriptCompileRequestDto();
            requestDto.Options = _options;

            foreach (var inputFile in context.Input)
            {
                var inputFileInfo = inputFile.FileInfo;

                var ext = System.IO.Path.GetExtension(inputFileInfo.Name);
                if (!string.IsNullOrEmpty(ext) && ext.ToLowerInvariant() == ".ts")
                {
                    // var inputFileInfo = inputFile.FileInfo;
                    var contents = inputFileInfo.ReadAllContent();
                    requestDto.Files.Add(inputFile.GetPath(), contents);

                    // tsFiles.Add(inputFile);
                }
                else
                {
                    // allow file to flow through pipeline untouched as we aren't interested in doing anything to non.ts files.
                    context.AddOutput(inputFile);
                }
            }

            if (!requestDto.Files.Any())
            {
                return;
            }

            // read script from embedded resource and use string as temp file:
            Assembly assy = ReflectionUtils.GetAssemblyFromType(this.GetType());
            var script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-typescript.js");
            var scriptContent = script.ReadAllContent();


            using (var nodeScript = new StringAsTempFile(scriptContent))
            {

                var result = await _nodeServices.InvokeAsync<TypeScriptCompileResult>(nodeScript.FileName, requestDto);

                if (result.Errors != null && result.Errors.Any())
                {
                    // Throwing an exception will halt further processing of the pipeline.
                    var typescriptCompilationException = new TypeScriptCompileException("Could not compile typescript due to compilation errors.", result.Errors);
                    throw typescriptCompilationException;
                }

                foreach (var output in result.Sources)
                {
                    var subPathInfo = SubPathInfo.Parse(output.Key);
                    var outputFileInfo = new StringFileInfo(output.Value, subPathInfo.FileName);
                    context.AddOutput(new SourceFile(outputFileInfo, subPathInfo.Directory));
                }

            }

        }

        public class TypeScriptCompileResult
        {
            public Dictionary<string, string> Sources { get; set; }
            public TypescripCompileError[] Errors { get; set; }
            public string Message { get; set; }
        }

        public class TypescripCompileError
        {
            public int Char { get; set; }
            public string File { get; set; }
            public int Line { get; set; }
            public string Message { get; set; }

        }

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

    public class TypeScriptCompileException : Exception
    {
        public TypeScriptCompileException() : base()
        {
            Errors = new List<TypeScriptCompilePipe.TypescripCompileError>();
        }
        public TypeScriptCompileException(string message, IEnumerable<TypeScriptCompilePipe.TypescripCompileError> errors) : base(message)
        {
            Errors = new List<TypeScriptCompilePipe.TypescripCompileError>();
            Errors.AddRange(errors);
        }

        public List<TypeScriptCompilePipe.TypescripCompileError> Errors { get; set; }

    }


}