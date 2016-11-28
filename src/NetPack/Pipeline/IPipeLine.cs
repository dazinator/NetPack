using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using NetPack.Pipes;
using Microsoft.Extensions.FileProviders;

namespace NetPack.Pipeline
{
    public interface IPipeLine
    {
        IDirectory Directory { get; set; }

        List<PipeConfiguration> Pipes { get; }

        Task ProcessAsync(CancellationToken cancelationToken);

        IFileProvider FileProvider { get; set; }

        //PipelineOutput Flush(TimeSpan? timeout = null);

        //bool IsWatching { get; }

        int FlushCount { get; }
        bool HasFlushed { get; }
        string RequestPath { get; set; }

        // bool IsFlushing { get; }
        void Initialise();
    }
}