using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;

namespace NetPack.Pipeline
{
    /// <summary>
    /// Provides access to all pipelines.
    /// </summary>
    public class PipelineManager
    {

        private readonly IHostingEnvironment _hostingEnv;

        public PipelineManager(IHostingEnvironment env)
        {
            _hostingEnv = env;
            PipeLines = new List<IPipeLine>();
        }

        public List<IPipeLine> PipeLines { get; set; }

        public void AddPipeLine(IPipeLine pipeline)
        {
            // Trigger the pipeline to be flushed if it hasn't already.
            // we want to block becausewe dont want the app to finish starting
            // before all assets have been processed..
            if (!pipeline.HasFlushed)
            {
                pipeline.FlushAsync().Wait(new TimeSpan(0,5,0));
               // await pipeline.FlushAsync();
            }
            PipeLines.Add(pipeline);
        }
        
    }
}