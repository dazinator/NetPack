using System;
using Microsoft.Extensions.FileProviders;

namespace NetPack.Pipes
{
    public interface IPipelineContext
    {
        /// <summary>
        /// The files provided to this pipe as input, for processing.
        /// </summary>
        SourceFile[] Input { get; }

        void AddOutput(SourceFile info);

    }
}