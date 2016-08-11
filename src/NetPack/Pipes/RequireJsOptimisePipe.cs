using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using NetPack.Extensions;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Utils;

namespace NetPack.Pipes
{
    public class RequireJsOptimisePipe : IPipe
    {
        private INodeServices _nodeServices;
        private RequireJsOptimisationPipeOptions _options;
        private IEmbeddedResourceProvider _embeddedResourceProvider;

        public RequireJsOptimisePipe(INodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider) : this(nodeServices, embeddedResourceProvider, new RequireJsOptimisationPipeOptions())
        {

        }

        public RequireJsOptimisePipe(INodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, RequireJsOptimisationPipeOptions options)
        {
            _nodeServices = nodeServices;
            _embeddedResourceProvider = embeddedResourceProvider;
            _options = options;
        }


        public async Task ProcessAsync(IPipelineContext context, CancellationToken cancelationToken)
        {
            Assembly assy = this.GetType().GetAssemblyFromType();
            var script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-requirejs-optimise.js");
            var scriptContent = script.ReadAllContent();

            using (var nodeScript = new StringAsTempFile(scriptContent))
            {
                var optimiseRequest = new RequireJsOptimiseRequestDto();

                foreach (var file in context.Input)
                {
                    var fileContent = file.FileInfo.ReadAllContent();
                    //  var dir = file.Directory;
                    // var name = file.FileInfo.Name;

                    // expose all input files to the node process, so r.js can see them using fs.
                    optimiseRequest.Files.Add(new NodeInMemoryFile()
                    {
                        FileContents = fileContent,
                        FilePath = file.GetPath()
                    });

                }

                var result = await _nodeServices.InvokeAsync<RequireJsOptimiseResult>(nodeScript.FileName, optimiseRequest);
                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    throw new RequireJsOptimiseException(result.Error);
                }
            }
        }
    }

    public class RequireJsOptimiseRequestDto
    {
        public RequireJsOptimiseRequestDto()
        {
            Files = new List<NodeInMemoryFile>();
        }

        public List<NodeInMemoryFile> Files { get; set; }


        //public string TypescriptCode { get; set; }
        //public TypeScriptPipeOptions Options { get; set; }
        //public string FilePath { get; set; }

    }

    public class RequireJsOptimiseResult
    {
        public string Result { get; set; }
        public string Error { get; set; }
    }

    public class NodeInMemoryFile
    {
        public string FilePath { get; set; }
        public string FileContents { get; set; }
        // public string FileName { get; set; }

    }

    public class RequireJsOptimiseException : Exception
    {
        public RequireJsOptimiseException() : base()
        {

        }
        public RequireJsOptimiseException(string message) : base(message)
        {

        }
    }
}