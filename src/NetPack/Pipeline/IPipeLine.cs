using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetPack.Pipes;

namespace NetPack.Pipeline
{
    public interface IPipeLine
    {
        PipelineInput Input { get; }

        PipelineOutput Output { get; set; }

        List<IPipe> Pipes { get; }

        Task<PipelineOutput> FlushAsync();

        //PipelineOutput Flush(TimeSpan? timeout = null);

        bool IsWatching { get; }

        int FlushCount { get; }
        bool HasFlushed { get; }

       // bool IsFlushing { get; }
        void Initialise();
    }
}