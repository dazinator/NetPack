using System;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using Dazinator.AspNet.Extensions.FileProviders;

namespace NetPack.Pipeline
{
    public class PipelineInputBuilder
    {

        private readonly IFileProvider _sourceFileProvider;
        //private readonly IHostingEnvironment _hostingEnv;
        public PipelineInputBuilder(IFileProvider sourceFileProvider)
        {
            _sourceFileProvider = sourceFileProvider;
            // _sources = new Sources();
            Input = new PipelineInput(sourceFileProvider);
        }

        public PipelineInputBuilder Include(string pattern)
        {

            Input.AddInclude(pattern);
            return this;
        }
        
        public PipelineInput Input { get; set; }

    }
}