using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NetPack.Extensions;
using NetPack.Node.Dto;
using NetPack.Pipeline;
using NetPack.Utils;
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Rollup
{
    public class RollupPipe : BasePipe
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

        public RollupPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<RollupPipe> logger, RollupInputOptions inputOptions, RollupOutputFileOptions[] outputOptions, string name = "Rollup") : base(name)
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
                return new StringAsTempFile(name, () =>
                {
                    string scriptContent = _rollupScriptGenerator.Value.GenerateScript(_inputOptions);
                    return scriptContent;
                });
            });
        }


        public override async Task ProcessAsync(PipeState state, CancellationToken cancelationToken)
        {
            RollupRequest optimiseRequest = new RollupRequest();
            var inputFiles = state.GetInputFiles();
            foreach (FileWithDirectory file in inputFiles)
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

            try
            {
                _logger.LogInformation("Invoking Rollup build with {FileCount} files", optimiseRequest.Files.Count);
                
                RollupResponse response = await _nodeServices.InvokeExportAsync<RollupRequest, RollupResponse>(_script.Value, "build", optimiseRequest, cancelationToken);
                
                _logger.LogInformation("Rollup build completed, response has {ResultCount} results", response?.Results?.Count ?? 0);
                cancelationToken.ThrowIfCancellationRequested();
                
                foreach (RollupOutputFileOptions output in _outputOptions)
                {
                    _logger.LogInformation("Processing output file: {OutputFile}", output.File);
                    var outputResults = response.Results[output.File];

                    // Ensure the file path starts with '/' for PathString compatibility
                    string filePath = output.File;
                    if (!filePath.StartsWith("/"))
                    {
                        filePath = "/" + filePath;
                    }
                    var filePathInfo = PathStringHelper.SplitPath(filePath);
                    
                  // PathStringHelper.GetPathAndFilename(output.File, out PathString rootPath, out string outputFileName);
                    
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
                        state.AddStringFile(filePathInfo.Directory, outputItem.Code.ToString(), filePathInfo.FileName);
                        if (outputItem.SourceMap != null)
                        {
                            string json =  JsonSerializer.Serialize(outputItem.SourceMap);
                            state.AddStringFile(filePathInfo.Directory, json, filePathInfo.FileName + ".map");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Rollup processing");
                throw;
            }          

        }
    }
}