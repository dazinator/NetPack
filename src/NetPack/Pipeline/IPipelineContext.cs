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

        /// <summary>
        /// Returns the previous files that were processed.
        /// </summary>
        FileWithDirectory[] PreviousInputFiles { get; set; }

        /// <summary>
        /// Returns the version of the file that was processed last time.
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        FileWithDirectory GetPreviousVersionOfFile(FileWithDirectory fileWithDirectory);

        /// <summary>
        /// Returns whether the file is different from the version that was processed last time.
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        bool IsDifferentFromLastTime(FileWithDirectory fileWithDirectory);

        /// <summary>
        /// Returns all the files that are detected as inputs for processing.
        /// </summary>
        FileWithDirectory[] InputFiles { get; set; }

        //  SourceFile[] GetFilesByExtension(string fileExtensionIncludingDotPrefix);

    }
}