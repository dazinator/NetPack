using System;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;

namespace NetPack.Pipeline
{
    public interface IPipelineContext
    {

        string BaseRequestPath { get; }

        void AddOutput(string directory, IFileInfo info);

        IDirectory Output { get; set; }

        IFileProvider FileProvider { get; set; }

        //  SourceFile[] GetFilesByExtension(string fileExtensionIncludingDotPrefix);

    }
}