using Dazinator.AspNet.Extensions.FileProviders;
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

namespace NetPack.Rollup
{
    public class RollupPipe : IPipe
    {
        private INetPackNodeServices _nodeServices;
        private readonly RollupPipeOptions _options;
        private IEmbeddedResourceProvider _embeddedResourceProvider;
        private readonly ILogger<RollupPipe> _logger;
        private Lazy<StringAsTempFile> _script = null;

        public RollupPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupPipe> logger) : this(nodeServices, embeddedResourceProvider, logger, new RollupPipeOptions())
        {

        }

        public RollupPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupPipe> logger, RollupPipeOptions options)
        {
            _nodeServices = nodeServices;
            _embeddedResourceProvider = embeddedResourceProvider;
            _options = options;
            _logger = logger;
            _script = new Lazy<StringAsTempFile>(() =>
            {
                Assembly assy = GetType().GetAssemblyFromType();
                Microsoft.Extensions.FileProviders.IFileInfo script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-requirejs-optimise.js");
                string scriptContent = script.ReadAllContent();
                return _nodeServices.CreateStringAsTempFile(scriptContent);
            });
        }


        public async Task ProcessAsync(PipeContext context, CancellationToken cancelationToken)
        {
            RollupRequest optimiseRequest = new RollupRequest();

            foreach (FileWithDirectory file in context.InputFiles)
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

            RollupResponse result = await _nodeServices.InvokeAsync<RollupResponse>(_script.Value.FileName, optimiseRequest);
            foreach (NodeInMemoryFile file in result.Files)
            {
                string filePath = file.Path.Replace('\\', '/');
                SubPathInfo subPathInfo = SubPathInfo.Parse(filePath);
                PathString dir = subPathInfo.Directory.ToPathString();
                context.AddOutput(dir, new StringFileInfo(file.Contents, subPathInfo.Name));
            }
        }
    }
}