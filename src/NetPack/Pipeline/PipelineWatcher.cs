using Microsoft.Extensions.Logging;
using NetPack.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Pipeline
{
    public class PipelineWatcher : IPipelineWatcher
    {
        private readonly CancellationTokenSource _tokenSource;
        private List<IPipeLine> _pipelines = new List<IPipeLine>();
        private readonly Task _monitoringTask = null;
        private ILogger<PipelineWatcher> _logger = null;


        public PipelineWatcher(ILogger<PipelineWatcher> logger)
        {
            _logger = logger;
        }

        public void WatchPipeline(IPipeLine pipeline, int watchTriggerDelay)
        {
            _pipelines.Add(pipeline);

            foreach (PipeProcessor pipe in pipeline.Pipes)
            {
                foreach (string include in pipe.Input.GetIncludes())
                {
                    StateInfo state = new StateInfo() { Input = include, PipeContext = pipe, Pipeline = pipeline };
                    WatchInput(state, watchTriggerDelay);
                }
            }
        }

        private IDisposable WatchInput(StateInfo state, int watchTriggerDelay)
        {

            Guid key = Guid.NewGuid();
            Microsoft.Extensions.FileProviders.IFileProvider fileProvider = state.Pipeline.Context.FileProvider;

            IDisposable disposable = ChangeTokenHelper.OnChangeDebounce<StateInfo>(() => fileProvider.Watch(state.Input), (s) =>
              {
                  DateTime changeTime = DateTime.UtcNow;
                  _logger.LogInformation("Changed signalled @ {0} for {1}, key: {2}, name: {name}", changeTime, s.Input, key, s.PipeContext.Pipe?.Name ?? string.Empty);

                  // process the pipe with changed inputs.                 
                  s.Pipeline.ProcessPipe(s.PipeContext, CancellationToken.None);
              }, state, watchTriggerDelay);

            return disposable;
        }

        protected class StateInfo
        {
            public string Input { get; set; }
            public IPipeLine Pipeline { get; set; }
            public PipeProcessor PipeContext { get; set; }
        }
    }
}