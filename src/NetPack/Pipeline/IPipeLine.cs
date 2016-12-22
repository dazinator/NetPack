using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using NetPack.RequireJs;
using Microsoft.Extensions.FileProviders;

namespace NetPack.Pipeline
{
    public interface IPipeLine
    {
        IDirectory ProcessedOutputDirectory { get; set; }

        List<PipeConfiguration> Pipes { get; }

        /// <summary>
        /// Processes all pipes in the pipeline.
        /// </summary>
        /// <param name="cancelationToken"></param>
        /// <returns></returns>
        Task ProcessAsync(CancellationToken cancelationToken);

        /// <summary>
        /// Processes only the specified pipes.
        /// </summary>
        /// <param name="pipes"></param>
        /// <param name="none"></param>
        /// <returns></returns>
        Task ProcessPipesAsync(IEnumerable<PipeConfiguration> pipes, CancellationToken none);

        IFileProvider EnvironmentFileProvider { get; set; }

        IFileProvider ProcessedOutputFileProvider { get; set; }

        IFileProvider InputAndOutputFileProvider { get; set; }

        IDirectory SourcesOutputDirectory { get; set; }

        IFileProvider SourcesFileProvider { get; set; }

        IFileProvider WebrootFileProvider { get; set; }

        //PipelineOutput Flush(TimeSpan? timeout = null);

        //bool IsWatching { get; }

        int FlushCount { get; }
        bool HasFlushed { get; }
        string BaseRequestPath { get; set; }

        // bool IsFlushing { get; }
        void Initialise();
        IEnumerable<PipeConfiguration> GetDirtyPipes();
    }
}