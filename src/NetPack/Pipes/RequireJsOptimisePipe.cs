using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
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


        public async Task ProcessAsync(IPipelineContext context)
        {
            // todo: read script from embedded resource and use string as temp file:
            //  _entryPointScript = new StringAsTempFile("some script");
            Assembly assy = ReflectionUtils.GetAssemblyFromType(this.GetType());
            var script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-requirejs-optimise.js");
            var scriptContent = script.ReadAllContent();

            using (var nodeScript = new StringAsTempFile(scriptContent))
            {
                var optimiseRequest = new RequireJsOptimiseRequestDto();
                var result = await _nodeServices.InvokeAsync<RequireJsOptimiseResult>(nodeScript.FileName, optimiseRequest);
            }
        }
    }

    public class RequireJsOptimiseRequestDto
    {
        //public string TypescriptCode { get; set; }
        //public TypeScriptPipeOptions Options { get; set; }
        //public string FilePath { get; set; }

    }

    public class RequireJsOptimiseResult
    {
        public string Something { get; set; }
    }
}