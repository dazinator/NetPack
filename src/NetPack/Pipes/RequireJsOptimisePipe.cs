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


        public async Task ProcessAsync(IPipelineContext context, FileWithDirectory[] input, CancellationToken cancelationToken)
        {
            Assembly assy = this.GetType().GetAssemblyFromType();
            var script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-requirejs-optimise.js");
            var scriptContent = script.ReadAllContent();

            using (var nodeScript = new StringAsTempFile(scriptContent))
            {
                var optimiseRequest = new RequireJsOptimiseRequestDto();

                foreach (var file in input)
                {
                    var fileContent = file.FileInfo.ReadAllContent();
                    //  var dir = file.Directory;
                    // var name = file.FileInfo.Name;

                    // expose all input files to the node process, so r.js can see them using fs.
                    optimiseRequest.Files.Add(new NodeInMemoryFile()
                    {
                        FileContents = fileContent,
                        FilePath = file.FileSubPath
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

    public class ModuleInfo
    {
        public ModuleInfo()
        {
            Exclude = new List<string>();
            ExcludeShallow = new List<string>();
            Include = new List<string>();
        }
        /// <summary>
        /// The name of the AMD modul - i.e "foo/bar/bop",
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// create: true can be used to create the module layer at the given
        /// name, if it does not already exist in the source location. If
        /// there is a module at the source location with this name, then
        /// create: true is superfluous.
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// The modules that should be excluded from the built file (including that modules dependencies)
        /// </summary>
        public List<string> Exclude { get; set; }

        /// <summary>
        /// Used to specify a specific module be excluded
        /// from the built module file. excludeShallow means just exclude that
        /// specific module, but if that module has nested dependencies that are
        /// part of the built file, keep them in there. This is useful during
        /// development when you want to have a fast bundled set of modules, but
        /// just develop/debug one or two modules at a time.
        /// </summary>
        public List<string> ExcludeShallow { get; set; }


        /// <summary>
        /// This moduels whose dependencies (and their depedencies etc) should be combined into one file.
        /// </summary>
        public List<string> Include { get; set; }

        /// <summary>
        /// Shows the use insertRequire (first available in 2.0):
        /// </summary>
        public string InsertRequire { get; set; }


    }
    public class RequireJsOptimiseRequestDto
    {
        public RequireJsOptimiseRequestDto()
        {
            Files = new List<NodeInMemoryFile>();
            Modules = new List<ModuleInfo>();
        }

        public List<NodeInMemoryFile> Files { get; set; }

        public List<ModuleInfo> Modules { get; set; }

        public string MainConfigFile { get; set; }

        public string BaseUrl { get; set; }

        public string Dir { get; set; }




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