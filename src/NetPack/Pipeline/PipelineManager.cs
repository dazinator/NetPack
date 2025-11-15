using System.Collections.Generic;
using Dazinator.Extensions.FileProviders.PrependBasePath;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace NetPack.Pipeline
{
    /// <summary>
    /// Provides access to all pipelines.
    /// </summary>
    public class PipelineManager
    {

        private readonly IHostingEnvironment _hostingEnv;

        public PipelineManager(IHostingEnvironment env, IPipelineWatcher watcher)
        {
            _hostingEnv = env;
            Watcher = watcher;
            PipeLines = new Dictionary<string, IPipeLine>();
        }

        public IPipelineWatcher Watcher { get; set; }

        public Dictionary<string, IPipeLine> PipeLines { get; set; }

        public void AddPipeLine(string key, IPipeLine pipeline, bool watch, int watchTriggerDelay)
        {           
            PipeLines.Add(key, pipeline);
            SetupPipeline(pipeline, watch, watchTriggerDelay);
        }

        private void SetupPipeline(IPipeLine pipeline, bool watch, int watchTriggerDelay)
        {
            //  var pipeLineWatcher = appBuilder.ApplicationServices.GetService<IPipelineWatcher>();

            var outputFileProvider = pipeline.WebrootFileProvider;
            if (!string.IsNullOrWhiteSpace(pipeline.Context.BaseRequestPath))
            {
                outputFileProvider = new PrependBasePathFileProvider(pipeline.Context.BaseRequestPath, outputFileProvider);
            }


            if (_hostingEnv.WebRootFileProvider == null || _hostingEnv.WebRootFileProvider is NullFileProvider)
            {
                _hostingEnv.WebRootFileProvider = outputFileProvider;
            }
            else
            {
                var composite = new CompositeFileProvider(_hostingEnv.WebRootFileProvider, outputFileProvider);
                _hostingEnv.WebRootFileProvider = composite;
            }

            pipeline.Initialise();

            if (watch)
            {
                Watcher.WatchPipeline(pipeline, watchTriggerDelay);
            }

        }

        IPipeLine GetPipeline(string name)
        {
            return PipeLines[name];
            // var pipeline = 
        }


    }
}