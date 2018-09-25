using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.NodeServices;
using NetPack.Extensions;
using NetPack.Pipeline;
using NetPack.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Typescript
{
    public class TypeScriptCompilePipe : BasePipe, IDisposable
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
                Assembly assy = GetType().GetAssemblyFromType();
                Microsoft.Extensions.FileProviders.IFileInfo script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-typescript.js");
                string scriptContent = script.ReadAllContent();

                return _nodeServices.CreateStringAsTempFile(scriptContent);
            });
        }




        public override async Task ProcessAsync(PipeContext context, CancellationToken cancelationToken)
        {
            TypescriptCompileRequestDto requestDto = new TypescriptCompileRequestDto();
            requestDto.Options = _options;

            //var pipeContext = context.PipeContext;
            // var requestLocks = new List<IDisposable>();           

            bool isSingleOutput = !string.IsNullOrWhiteSpace(_options.OutFile);

            if (isSingleOutput)
            {
                PathString outFilePath;               
                if (_options.OutFile.StartsWith("/"))
                {
                    outFilePath = _options.OutFile;
                }
                else
                {
                    outFilePath = new PathString($"/{_options.OutFile}");
                }               
                context.AddBlock(outFilePath);
            }

            foreach (FileWithDirectory inputFileInfo in context.InputFiles)
            {
                if (context.IsInputDifferentFromLastTime(inputFileInfo))
                {
                    if (!isSingleOutput)
                    {
                        var outFileName = Path.ChangeExtension(inputFileInfo.UrlPath, ".js");
                        context.AddBlock(outFileName);
                    }

                    string contents = inputFileInfo.FileInfo.ReadAllContent();
                    requestDto.Files.Add(inputFileInfo.UrlPath, contents);
                }              

                requestDto.Inputs.Add(inputFileInfo.UrlPath);
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


                StringAsTempFile nodeScript = _script.Value;
                TypeScriptCompileResult result = await _nodeServices.InvokeExportAsync<TypeScriptCompileResult>(nodeScript.FileName, "build", requestDto);

                if (result.Errors != null && result.Errors.Any())
                {
                    // Throwing an exception will halt further processing of the pipeline.
                    TypeScriptCompileException typescriptCompilationException = new TypeScriptCompileException("Could not compile typescript due to compilation errors.", result.Errors);
                    throw typescriptCompilationException;
                }

                foreach (KeyValuePair<string, string> output in result.Sources)
                {
                    SubPathInfo subPathInfo = SubPathInfo.Parse(output.Key);
                    StringFileInfo outputFileInfo = new StringFileInfo(output.Value, subPathInfo.Name);
                   
                    context.AddUpdateOutputFile(new FileWithDirectory() { Directory = subPathInfo.Directory.ToPathString(), FileInfo = outputFileInfo });                    
                  
                }

                // also, if source maps are enabled, but source is not inlined in the source map, then the 
                // source file needs to be output so it can be served up to the browser.              
                if (_options.SourceMap.GetValueOrDefault() && !_options.InlineSources)
                {
                    foreach (FileWithDirectory inputFileInfo in context.InputFiles)
                    {
                        //  context.AllowServe(inputFileInfo);
                        //if (context.SourcesOutput.GetFile(inputFileInfo.FileSubPath) == null)
                        //{

                       // inputFileInfo.Directory, inputFileInfo.FileInfo
                        context.AddUpdateSourceOutput(inputFileInfo);
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