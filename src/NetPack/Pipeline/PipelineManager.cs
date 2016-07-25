using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Composite;
using Microsoft.Extensions.Options;

namespace NetPack.Pipeline
{
    /// <summary>
    /// Provides access to all pipelines.
    /// </summary>
    public class PipelineManager
    {

        private readonly IHostingEnvironment _hostingEnv;
      //  private readonly IOptions<StaticFileOptions>  _staticFilesOptions;


        public PipelineManager(IHostingEnvironment env)
        {
            _hostingEnv = env;
            PipeLines = new List<IPipeLine>();
            //_staticFilesOptions = options;

// wrap the static files file provider with one that resolve netpack pipeline outputs.
           // var existingStaticFilesProvider = options.Value.FileProvider ?? _hostingEnv.WebRootFileProvider;

// give a content directory, and a webroot directory like so:
//c:\\somepath\\somedir
//c:\\somepath\\somedir\\webroot\\static\\

// we have input files specified with a path relative to the content directory like this:

//webroot/static/somefile.ts


        }

       /// public List<IPipeLine> PipeLines { get; set; }

        public List<IPipeLine> PipeLines { get; set; }

        public void AddPipeLine(IPipeLine pipeline)
        {
            pipeline.Initialise();
            PipeLines.Add(pipeline);
        }

        
    }
}