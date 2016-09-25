using System;
using Microsoft.Extensions.FileProviders;
using NetPack.File;

namespace NetPack.Pipeline
{
    public interface IPipelineContext
    {

        SourceFile FindFile(SubPathInfo subPath);


        /// <summary>
        /// The files provided to this pipe as input, for processing.
        /// </summary>
        SourceFile[] Input { get; }

        void AddOutput(SourceFile info);

        SourceFile[] ApplyFilter(Predicate<SourceFile> filter);

        string BaseRequestPath { get; }

        //  SourceFile[] GetFilesByExtension(string fileExtensionIncludingDotPrefix);

    }
}