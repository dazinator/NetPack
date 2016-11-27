using System;
using System.Collections.Generic;
using NetPack.File;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;

namespace NetPack.Pipeline
{
    public class PipelineContext : IPipelineContext
    {

        public PipelineContext(IFileProvider fileProvider, IDirectory directory, string baseRequestPath)
        {
            FileProvider = fileProvider;
            Output = directory;
            BaseRequestPath = baseRequestPath;
            //Input = input;
        }

        public string BaseRequestPath { get; }

        public void AddOutput(string directory, IFileInfo info)
        {
            Output.AddFile(directory, info);
        }

        public IDirectory Output { get; set; }

        public IFileProvider FileProvider { get; set; }

    }
}