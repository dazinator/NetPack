using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Utils;
using System.Reflection;

namespace NetPack.Pipes
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

        public async Task ProcessAsync(IPipelineContext context)
        {
            // todo: read script from embedded resource and use string as temp file:

            Assembly assy = ReflectionUtils.GetAssemblyFromType(this.GetType());
            var script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-typescript.js");
            var scriptContent = script.ReadAllContent();

            using (var nodeScript = new StringAsTempFile(scriptContent))
            {
                foreach (var inputFile in context.Input)
                {
                    // only interested in typescript files.
                    var inputFileInfo = inputFile.FileInfo;

                    var ext = System.IO.Path.GetExtension(inputFileInfo.Name);
                    if (!string.IsNullOrEmpty(ext) && ext.ToLowerInvariant() == ".ts")
                    {
                        var contents = inputFileInfo.ReadAllContent();

                        var requestDto = new TypescriptCompileRequestDto();
                        requestDto.TypescriptCode = contents;
                        requestDto.Options = _options;
                        requestDto.FilePath = inputFile.GetPath();

                        var result = await _nodeServices.InvokeAsync<TypeScriptCompileResult>(nodeScript.FileName, requestDto);

                        var fileName = System.IO.Path.GetFileNameWithoutExtension(inputFileInfo.Name);
                        var outputFileName = fileName + ".js";

                        // output the js file in the same directory.
                        var fileinfo = new StringFileInfo(result.Code, outputFileName);
                        context.AddOutput(new SourceFile(fileinfo, inputFile.Directory));

                    }
                    else
                    {
                        // allow file to flow through pipeline untouched as its not a .ts file.
                        context.AddOutput(inputFile);
                    }

                }

            }

           
        }

        public class TypescriptCompileRequestDto
        {
            public string TypescriptCode { get; set; }
            public TypeScriptPipeOptions Options { get; set; }
            public string FilePath { get; set; }

        }
    }

   
}