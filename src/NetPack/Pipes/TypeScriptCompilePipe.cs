using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using NetPack.Pipeline;
using NetPack.Requirements;

namespace NetPack.Pipes
{
    public class TypeScriptCompilePipe : IPipe
    {
        private INodeServices _nodeServices;
        private NodeJsRequirement _nodeJsRequirement;
        private TypeScriptPipeOptions _tsOptions;

        public TypeScriptCompilePipe(INodeServices nodeServices, NodeJsRequirement nodeJsRequirement) : this(nodeServices, nodeJsRequirement, new TypeScriptPipeOptions())
        {

        }

        public TypeScriptCompilePipe(INodeServices nodeServices, NodeJsRequirement nodeJsRequirement, TypeScriptPipeOptions tsOptions)
        {
            _nodeServices = nodeServices;
            _nodeJsRequirement = nodeJsRequirement;
            _tsOptions = new TypeScriptPipeOptions();
        }

        public async Task ProcessAsync(IPipelineContext context)
        {

            //  var resultString = await _nodeServices.InvokeAsync<string>("./netpack-typescript", "yo");

            foreach (var inputFile in context.Input)
            {
                // only interested in typescript files.
                var inputFileInfo = inputFile.FileInfo;

                var ext = System.IO.Path.GetExtension(inputFileInfo.Name);
                if (!string.IsNullOrEmpty(ext) && ext.ToLowerInvariant() == ".ts")
                {
                    using (var reader = new StreamReader(inputFileInfo.CreateReadStream()))
                    {

                        var requestDto = new TypescriptCompileRequestDto();
                        requestDto.TypescriptCode = reader.ReadToEnd();
                        requestDto.Options = _tsOptions;
                        requestDto.FilePath = inputFile.GetPath();

                        var result = await _nodeServices.InvokeAsync<TypeScriptCompileResult>("./netpack-typescript", requestDto);

                        var fileName = System.IO.Path.GetFileNameWithoutExtension(inputFileInfo.Name);
                        var outputFileName = fileName + ".js";

                        // output the js file in the same directory.
                        var fileinfo = new StringFileInfo(result.Code, outputFileName);
                        context.AddOutput(new SourceFile(fileinfo, inputFile.Directory));
                    }
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