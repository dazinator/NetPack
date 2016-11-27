using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using NetPack.Pipes;

namespace NetPack.Pipeline
{
    public interface IPipeLine
    {
        IDirectory Output { get; set; }

        List<PipeConfiguration> Pipes { get; }

        Task FlushAsync(CancellationToken cancelationToken);

        //PipelineOutput Flush(TimeSpan? timeout = null);

        //bool IsWatching { get; }

        int FlushCount { get; }
        bool HasFlushed { get; }
        string RequestPath { get; set; }

        // bool IsFlushing { get; }
        void Initialise();
    }
}