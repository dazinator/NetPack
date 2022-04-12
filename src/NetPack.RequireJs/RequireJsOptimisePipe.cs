using Dazinator.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.Logging;
using NetPack.Extensions;
using NetPack.Node.Dto;
using NetPack.Pipeline;
using NetPack.Utils;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.RequireJs
{
    public class RequireJsOptimisePipe : BasePipe
    {
        private INetPackNodeServices _nodeServices;
        private readonly RequireJsOptimisationPipeOptions _options;
        private IEmbeddedResourceProvider _embeddedResourceProvider;
        private readonly ILogger<RequireJsOptimisePipe> _logger;
        private Lazy<StringAsTempFile> _script = null;

        public RequireJsOptimisePipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RequireJsOptimisePipe> logger) : this(nodeServices, embeddedResourceProvider, logger, new RequireJsOptimisationPipeOptions())
        {

        }

        public RequireJsOptimisePipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RequireJsOptimisePipe> logger, RequireJsOptimisationPipeOptions options, string name = "RequireJs Optimise") : base(name)
        {
            _nodeServices = nodeServices;
            _embeddedResourceProvider = embeddedResourceProvider;
            _options = options;
            _script = new Lazy<StringAsTempFile>(() =>
            {
                // Todo replace with embedded manifest provider
                Assembly assy = GetType().GetAssemblyFromType();
                Microsoft.Extensions.FileProviders.IFileInfo script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-requirejs-optimise.js");
                string scriptContent = script.ReadAllContent();
                return _nodeServices.CreateStringAsTempFile(scriptContent);
            });
        }


        public override async Task ProcessAsync(PipeState context, CancellationToken cancelationToken)
        {

            // var pipeContext = context.PipeContext;


            RequireJsOptimiseRequestDto optimiseRequest = new RequireJsOptimiseRequestDto();

            var inputFiles = context.GetInputFiles();
            foreach (FileWithDirectory file in inputFiles)
            {
                string fileContent = file.FileInfo.ReadAllContent();
                //  var dir = file.Directory;
                // var name = file.FileInfo.Name;

                // expose all input files to the node process, so r.js can see them using fs.
                optimiseRequest.Files.Add(new NodeInMemoryFile()
                {
                    Contents = fileContent,
                    Path = file.UrlPath.ToString().TrimStart(new char[] { '/' })
                });
            }

            optimiseRequest.Options = _options;


            cancelationToken.ThrowIfCancellationRequested();
            RequireJsOptimiseResult result = await _nodeServices.InvokeAsync<RequireJsOptimiseResult>(_script.Value.FileName, optimiseRequest);
            foreach (NodeInMemoryFile file in result.Files)
            {
                PathStringUtils.GetPathAndFilename(file.Path, out var directory, out var fileName);
                context.AddOutput(directory, new StringFileInfo(file.Contents, fileName));
            }

            //if (!string.IsNullOrWhiteSpace(result.Error))
            //{
            //    throw new RequireJsOptimiseException(result.Error);
            //}



        }



    }
}