using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.NodeServices;
using NetPack.Extensions;
using NetPack.Pipeline;
using NetPack.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace NetPack.RequireJs
{
    public class RequireJsOptimisePipe : IPipe
    {
        private INetPackNodeServices _nodeServices;
        private RequireJsOptimisationPipeOptions _options;
        private IEmbeddedResourceProvider _embeddedResourceProvider;
        private ILogger<RequireJsOptimisePipe> _logger;

        public RequireJsOptimisePipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RequireJsOptimisePipe> logger) : this(nodeServices, embeddedResourceProvider, logger, new RequireJsOptimisationPipeOptions())
        {

        }

        public RequireJsOptimisePipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RequireJsOptimisePipe> logger, RequireJsOptimisationPipeOptions options)
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

                foreach (var file in context.InputFiles)
                {
                    var fileContent = file.FileInfo.ReadAllContent();
                    //  var dir = file.Directory;
                    // var name = file.FileInfo.Name;

                    // expose all input files to the node process, so r.js can see them using fs.
                    optimiseRequest.Files.Add(new NodeInMemoryFile()
                    {
                        Contents = fileContent,
                        Path = file.FileSubPath.TrimStart(new char[] { '/' })
                    });
                }

                optimiseRequest.Options = _options;

                try
                {
                    var result = await _nodeServices.InvokeAsync<RequireJsOptimiseResult>(nodeScript.FileName, optimiseRequest);
                    foreach (var file in result.Files)
                    {
                        var filePath = file.Path.Replace('\\', '/');
                        var subPathInfo = SubPathInfo.Parse(filePath);
                        context.AddOutput(subPathInfo.Directory, new StringFileInfo(file.Contents, subPathInfo.Name));
                    }

                    //if (!string.IsNullOrWhiteSpace(result.Error))
                    //{
                    //    throw new RequireJsOptimiseException(result.Error);
                    //}
                }
                catch (Exception e)
                {

                    var jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(optimiseRequest, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    throw;
                }


            }
        }


    }
}