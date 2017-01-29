using System;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.NodeServices;
using NetPack.Extensions;
using NetPack.Pipeline;
using NetPack.RequireJs;
using NetPack.Utils;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Typescript
{
    public class TypeScriptCompilePipe : IPipe, IDisposable
    {
        private INetPackNodeServices _nodeServices;
        private TypeScriptPipeOptions _options;
        private IEmbeddedResourceProvider _embeddedResourceProvider;
        private Lazy<StringAsTempFile> _script = null;


        public TypeScriptCompilePipe(INetPackNodeServices nodeServices,
            IEmbeddedResourceProvider embeddedResourceProvider) : this(nodeServices, embeddedResourceProvider, new TypeScriptPipeOptions())
        {

        }

        public TypeScriptCompilePipe(INetPackNodeServices nodeServices,
            IEmbeddedResourceProvider embeddedResourceProvider,
            TypeScriptPipeOptions options)
        {
            _nodeServices = nodeServices;
            _embeddedResourceProvider = embeddedResourceProvider;
            _options = options;
            _script = new Lazy<StringAsTempFile>(() =>
            {
                Assembly assy = this.GetType().GetAssemblyFromType();
                var script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-typescript.js");
                var scriptContent = script.ReadAllContent();
                return new StringAsTempFile(scriptContent);
            });
        }




        public async Task ProcessAsync(IPipelineContext context, CancellationToken cancelationToken)
        {
            var requestDto = new TypescriptCompileRequestDto();
            requestDto.Options = _options;

            foreach (var inputFileInfo in context.InputFiles)
            {
                if (context.IsDifferentFromLastTime(inputFileInfo))
                {
                    var contents = inputFileInfo.FileInfo.ReadAllContent();
                    requestDto.Files.Add(inputFileInfo.FileSubPath, contents);
                }

                requestDto.Inputs.Add(inputFileInfo.FileSubPath);
            }

            if (!requestDto.Files.Any())
            {
                return;
            }

            try
            {
                // read script from embedded resource and use string as temp file:
                // Assembly assy = this.GetType().GetAssemblyFromType();
                // var script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-typescript.js");
                // var scriptContent = script.ReadAllContent();


                var nodeScript = _script.Value;
                var result = await _nodeServices.InvokeExportAsync<TypeScriptCompileResult>(nodeScript.FileName, "build", requestDto);

                if (result.Errors != null && result.Errors.Any())
                {
                    // Throwing an exception will halt further processing of the pipeline.
                    var typescriptCompilationException = new TypeScriptCompileException("Could not compile typescript due to compilation errors.", result.Errors);
                    throw typescriptCompilationException;
                }

                foreach (var output in result.Sources)
                {
                    var subPathInfo = SubPathInfo.Parse(output.Key);
                    var outputFileInfo = new StringFileInfo(output.Value, subPathInfo.Name);
                    context.AddOutput(subPathInfo.Directory, outputFileInfo);
                }

                // also, if source maps are enabled, but source is not inlined in the source map, then the 
                // source file needs to be output so it can be served up to the browser.              
                if (_options.SourceMap.GetValueOrDefault() && !_options.InlineSources)
                {
                    foreach (var inputFileInfo in context.InputFiles)
                    {
                        //  context.AllowServe(inputFileInfo);
                        //if (context.SourcesOutput.GetFile(inputFileInfo.FileSubPath) == null)
                        //{
                        context.AddSourceOutput(inputFileInfo.Directory, inputFileInfo.FileInfo);
                        // }
                        // else
                        // {
                        // source file is already being served.
                        //    }

                    }

                }

            }
            catch (System.Exception e)
            {

                throw;
            }

        }

        public void Dispose()
        {
            if (_script.IsValueCreated)
            {
                _script.Value.Dispose();
            }
        }
    }




}