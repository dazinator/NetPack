using Dazinator.AspNet.Extensions.FileProviders;
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
        private readonly RollupInputOptions _inputOptions;
        private readonly RollupOutputFileOptions _outputOptions;
        private IEmbeddedResourceProvider _embeddedResourceProvider;
        private readonly ILogger<RollupPipe> _logger;
        private Lazy<StringAsTempFile> _script = null;
        private readonly Lazy<RollupScriptGenerator> _rollupScriptGenerator;

        public RollupPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupPipe> logger) : this(nodeServices, embeddedResourceProvider, logger, new RollupInputOptions())
        {

        }

        public RollupPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupPipe> logger, RollupInputOptions options) : this(nodeServices, embeddedResourceProvider, logger, new RollupInputOptions(), new RollupOutputFileOptions())
        {            
        }

        public RollupPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupPipe> logger, RollupInputOptions inputOptions, RollupOutputFileOptions outputOptions)
        {
            _nodeServices = nodeServices;
            _embeddedResourceProvider = embeddedResourceProvider;
            _inputOptions = inputOptions;
            _outputOptions = outputOptions;
            _logger = logger;
            _rollupScriptGenerator = new Lazy<RollupScriptGenerator>(() => {
                Assembly assy = GetType().GetAssemblyFromType();
                var template = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/RollupTemplate.txt");
                return new RollupScriptGenerator(template);
            });

            _script = new Lazy<StringAsTempFile>(() =>
            {
                string scriptContent = _rollupScriptGenerator.Value.GenerateScript(_inputOptions);
                return _nodeServices.CreateStringAsTempFile(scriptContent);
            });
        }


        public async Task ProcessAsync(PipeContext context, CancellationToken cancelationToken)
        {
            RollupRequest optimiseRequest = new RollupRequest();

            foreach (FileWithDirectory file in context.InputFiles)
            {
                string fileContent = file.FileInfo.ReadAllContent();

                // expose all input files to the node process, so r.js can see them using fs.
                optimiseRequest.Files.Add(new NodeInMemoryFile()
                {
                    Contents = fileContent,
                    Path = file.UrlPath.ToString() //.TrimStart(new char[] { '/' })
                });
            }

            optimiseRequest.InputOptions = _inputOptions;
            optimiseRequest.OutputOptions = _outputOptions;

            RollupResponse response = await _nodeServices.InvokeExportAsync<RollupResponse>(_script.Value.FileName, "build", optimiseRequest);         
            var result = response.Result;
          
            context.AddOutput("/", new StringFileInfo(result.Code.ToString(), _outputOptions.File));
            if(result.SourceMap != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(result.SourceMap);                
                context.AddOutput("/", new StringFileInfo(json, _outputOptions.File + ".map"));
            }
        }
    }
}