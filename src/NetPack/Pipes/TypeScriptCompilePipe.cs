using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using NetPack.Pipes;
using NetPack.Requirements;

namespace NetPack.Tests
{
    public class TypeScriptCompilePipe : IPipe
    {
        private INodeServices _nodeServices;
        private NodeJsRequirement _nodeJsRequirement;

        public TypeScriptCompilePipe(INodeServices nodeServices, NodeJsRequirement nodeJsRequirement)
        {
            _nodeServices = nodeServices;
            _nodeJsRequirement = nodeJsRequirement;
        }

        public async Task ProcessAsync(IPipelineContext context)
        {
            foreach (var inputFile in context.Input)
            {
                // only interested in typescript files.
                var inputFileInfo = inputFile.FileInfo;

                var ext = System.IO.Path.GetExtension(inputFileInfo.Name);
                if (!string.IsNullOrEmpty(ext) && ext.ToLowerInvariant() == ".ts")
                {
                    using (var reader = new StreamReader(inputFileInfo.CreateReadStream()))
                    {
                        var text = reader.ReadToEnd();
                        var result = await _nodeServices.InvokeAsync<Result>("./netpack-typescript", text);

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

    public class Result
    {
        public string Code { get; set; }
    }
}