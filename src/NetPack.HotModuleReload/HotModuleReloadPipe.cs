using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetPack.Extensions;
using NetPack.Pipeline;
using NetPack.Utils;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.Extensions.FileProviders;

namespace NetPack.HotModuleReload
{
    public class HotModuleReloadPipe : BasePipe
    {
        private INetPackNodeServices _nodeServices;
        private readonly IOptions<HotModuleReloadOptions> _options;
        private IEmbeddedResourceProvider _embeddedResourceProvider;
        private readonly ILogger<HotModuleReloadPipe> _logger;
        private Lazy<string> _script = null;



        public HotModuleReloadPipe(INetPackNodeServices nodeServices, IEmbeddedResourceProvider embeddedResourceProvider, ILogger<HotModuleReloadPipe> logger, IOptions<HotModuleReloadOptions> options, string name = "Hot Module Reload"):base(name)
        {
            _nodeServices = nodeServices;
            _embeddedResourceProvider = embeddedResourceProvider;
            _options = options;
            _script = new Lazy<string>(() =>
            {
                Assembly assy = GetType().GetAssemblyFromType();
                Microsoft.Extensions.FileProviders.IFileInfo script = _embeddedResourceProvider.GetResourceFile(assy, "Embedded/netpack-madge-entry.js");
                string scriptContent = script.ReadAllContent();
                return scriptContent;
                //return new EmbeddedFileInfo(assy, "Embedded/netpack-madge-entry.js", "netpack-madge-entry.js");
                //return _nodeServices.CreateStringAsTempFile(scriptContent);
            });
        }


        public override async Task ProcessAsync(PipeState context, CancellationToken cancelationToken)
        {

            // get all changed inputs, we expect them to be modules of some kind.
            // then send them, and the options, to madge.
            // get back module and dependents for the changed module.
            // then publish a hmr reload event for changed module, with its dependant information.

            // browser implementation can take care of reloading the module including it's dependants.

          


        }



    }
}