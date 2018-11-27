using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.Logging;
using NetPack.Extensions;
using NetPack.Node.Dto;
using NetPack.Pipeline;
using NetPack.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Rollup
{
    public class RollupPipe : IPipe
    {
        private INetPackNodeServices _nodeServices;
        private readonly RollupInputOptions _inputOptions;
        private readonly RollupOutputFileOptions[] _outputOptions;
        private IEmbeddedResourceProvider _embeddedResourceProvider;
        private readonly ILogger<RollupPipe> _logger;
        private Lazy<StringAsTempFile> _script = null;
        private readonly Lazy<RollupScriptGenerator> _rollupScriptGenerator;

        public RollupPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupPipe> logger) : this(nodeServices, embeddedResourceProvider, logger, new RollupInputOptions())
        {

        }

        public RollupPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupPipe> logger, RollupInputOptions options) : this(nodeServices, embeddedResourceProvider, logger, new RollupInputOptions(), new RollupOutputFileOptions[] { })
        {
        }

        public RollupPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupPipe> logger, RollupInputOptions inputOptions, RollupOutputFileOptions[] outputOptions)
        {
            _nodeServices = nodeServices;
            _embeddedResourceProvider = embeddedResourceProvider;
            _inputOptions = inputOptions;
            _outputOptions = outputOptions;
            _logger = logger;
            _rollupScriptGenerator = new Lazy<RollupScriptGenerator>(() =>
            {
                Assembly assy = GetType().GetAssemblyFromType();
                Microsoft.Extensions.FileProviders.IFileInfo template = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/RollupTemplate.txt");
                return new RollupScriptGenerator(template);
            });

            _script = new Lazy<StringAsTempFile>(() =>
            {
                string scriptContent = _rollupScriptGenerator.Value.GenerateScript(_inputOptions);
                return _nodeServices.CreateStringAsTempFile(scriptContent);
            });
        }


        public async Task ProcessAsync(PipeState state, CancellationToken cancelationToken)
        {
            RollupRequest optimiseRequest = new RollupRequest();

            foreach (FileWithDirectory file in state.InputFiles)
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

            cancelationToken.ThrowIfCancellationRequested();

            RollupResponse response = await _nodeServices.InvokeExportAsync<RollupResponse>(_script.Value.FileName, "build", optimiseRequest);
            //Queue<RollupResult> results = new Queue<RollupResult>(response.Result);
            cancelationToken.ThrowIfCancellationRequested();
            
            foreach (RollupOutputFileOptions output in _outputOptions)
            {
                var outputResults = response.Results[output.File];
                
                PathStringUtils.GetPathAndFilename(output.File, out PathString rootPath, out string outputFileName);
                
                foreach (var outputItem in outputResults)
                {
                    if(outputItem.Modules != null)
                    {
                        foreach (var module in outputItem.Modules)
                        {
                            foreach (var export in module.Exports)
                            {

                            }
                        }
                    }
                   
                    state.AddOutput(rootPath, new StringFileInfo(outputItem.Code.ToString(), outputFileName));
                    if (outputItem.SourceMap != null)
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(outputItem.SourceMap);
                        state.AddOutput(rootPath, new StringFileInfo(json, outputFileName + ".map"));
                    }
                }

            }          

        }
    }
}