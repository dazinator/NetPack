using System;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace NetPack.Pipeline
{
    public interface IPipelineContext
    {

        PathString BaseRequestPath { get; }

        void AddGeneratedOutput(string directory, IFileInfo info);

        void AddSourceOutput(string directory, IFileInfo file);

        /// <summary>
        /// A directory where generated files are added.
        /// </summary>
        IDirectory GeneratedOutput { get; set; }

        /// <summary>
        /// A directory where source files that need to be served up should be added.
        /// </summary>
        IDirectory SourcesOutput { get; }

        IFileProvider FileProvider { get; set; }

        //  PathString GetRequestPath(PathString path);
        PathString GetRequestPath(string directory, IFileInfo fileInfo);

        PipeContext PipeContext { get; }

        ///// <summary>
        ///// Returns whether the file is different from the version that was processed last time.
        ///// </summary>
        ///// <param name="fileWithDirectory"></param>
        ///// <returns></returns>
        //bool IsDifferentFromLastTime(FileWithDirectory fileWithDirectory);






        //  SourceFile[] GetFilesByExtension(string fileExtensionIncludingDotPrefix);

    }
}