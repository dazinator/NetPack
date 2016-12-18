using System;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using Dazinator.AspNet.Extensions.FileProviders;

namespace NetPack.Pipeline
{
    public class PipelineInputBuilder
    {

        //   private readonly IFileProvider _sourceFileProvider;
        //private readonly IHostingEnvironment _hostingEnv;
        public PipelineInputBuilder()
        {
            //  _sourceFileProvider = sourceFileProvider;
            // _sources = new Sources();
            Input = new PipelineInput();
        }

        public PipelineInputBuilder Include(string pattern)
        {
            Input.AddInclude(pattern);
            return this;
        }

        public PipelineInputBuilder Exclude(string pattern)
        {
            Input.AddExclude(pattern);
            return this;
        }

        public PipelineInput Input { get; set; }

    }
}