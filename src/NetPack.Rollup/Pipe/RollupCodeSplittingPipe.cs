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
    public class RollupCodeSplittingPipe : IPipe
    {
        private INetPackNodeServices _nodeServices;
        private readonly RollupCodeSplittingInputOptions _inputOptions;
        private readonly RollupOutputOptions _outputOptions;
        private IEmbeddedResourceProvider _embeddedResourceProvider;
        private readonly ILogger<RollupCodeSplittingPipe> _logger;
        private Lazy<StringAsTempFile> _script = null;
        private readonly Lazy<RollupCodeSplittingScriptGenerator> _rollupScriptGenerator;

        public RollupCodeSplittingPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupCodeSplittingPipe> logger) : this(nodeServices, embeddedResourceProvider, logger, new RollupCodeSplittingInputOptions())
        {

        }

        public RollupCodeSplittingPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupCodeSplittingPipe> logger, RollupCodeSplittingInputOptions options) : this(nodeServices, embeddedResourceProvider, logger, new RollupCodeSplittingInputOptions(), new RollupOutputOptions())
        {
        }

        public RollupCodeSplittingPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupCodeSplittingPipe> logger, RollupCodeSplittingInputOptions inputOptions, RollupOutputOptions outputOptions)
        {
            _nodeServices = nodeServices;
            _embeddedResourceProvider = embeddedResourceProvider;
            _inputOptions = inputOptions;
            _outputOptions = outputOptions;
            _logger = logger;
            _rollupScriptGenerator = new Lazy<RollupCodeSplittingScriptGenerator>(() => {
                Assembly assy = GetType().GetAssemblyFromType();
                var template = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/RollupCodeSplittingTemplate.txt");
                return new RollupCodeSplittingScriptGenerator(template);
            });

            _script = new Lazy<StringAsTempFile>(() =>
            {
                string scriptContent = _rollupScriptGenerator.Value.GenerateScript(_inputOptions);
                return _nodeServices.CreateStringAsTempFile(scriptContent);
            });
        }


        public async Task ProcessAsync(PipeContext context, CancellationToken cancelationToken)
        {
            RollupCodeSplittingRequest optimiseRequest = new RollupCodeSplittingRequest();

            foreach (FileWithDirectory file in context.InputFiles)
            {
                string fileContent = file.FileInfo.ReadAllContent();
                //  var dir = file.Directory;
                // var name = file.FileInfo.Name;

                // expose all input files to the node process, so r.js can see them using fs.
                optimiseRequest.Files.Add(new NodeInMemoryFile()
                {
                    Contents = fileContent,
                    Path = file.UrlPath.ToString() //.TrimStart(new char[] { '/' })
                });
            }

            optimiseRequest.InputOptions = _inputOptions;
            optimiseRequest.OutputOptions = _outputOptions;

            RollupCodeSplittingResponse result = await _nodeServices.InvokeExportAsync<RollupCodeSplittingResponse>(_script.Value.FileName, "build", optimiseRequest);
            //if(result.Files!= null)
            //{
            //    foreach (NodeInMemoryFile file in result.Files)
            //    {
            //        string filePath = file.Path.Replace('\\', '/');
            //        SubPathInfo subPathInfo = SubPathInfo.Parse(filePath);
            //        PathString dir = subPathInfo.Directory.ToPathString();
            //        context.AddOutput(dir, new StringFileInfo(file.Contents, subPathInfo.Name));
            //    }
            //}



            var code = result.Code;
            var map = result.SourceMap;

            context.AddOutput("/", new StringFileInfo(code.ToString(), _outputOptions.File));
            //   context.AddOutput("/", new StringFileInfo(code.ToString(), _outputOptions.File));


        }
    }
}