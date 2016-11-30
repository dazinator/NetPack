using System;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.AspNetCore.Http;

namespace NetPack.Pipeline
{
    public interface IPipelineContext
    {

        PathString BaseRequestPath { get; }

        void AddOutput(string directory, IFileInfo info);

        IDirectory Output { get; set; }

        IFileProvider FileProvider { get; set; }

      //  PathString GetRequestPath(PathString path);
        PathString GetRequestPath(string directory, IFileInfo fileInfo);

        //  SourceFile[] GetFilesByExtension(string fileExtensionIncludingDotPrefix);

    }
}