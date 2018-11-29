using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
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
                    var token = WatchInput(state, watchTriggerDelay);
                }
            }
        }

        private IChangeToken WatchInput(StateInfo state, int watchTriggerDelay)
        {

            Guid key = Guid.NewGuid();
            var fileProvider = state.Pipeline.Context.FileProvider;

            IChangeToken token = fileProvider.Watch(state.Input);

            ChangeTokenHelper.OnChangeDelayed(() => fileProvider.Watch(state.Input), (s)=> {

                // Mark the input as changed.
                DateTime changeTime = DateTime.UtcNow;
                _logger.LogInformation("Changed signalled @ {0} for {1}, key: {2}", changeTime, s.Input, key);
                //  s.PipeContext.Input.LastChanged = changeTime;
                var targetPipes = new[] { s.PipeContext }.AsEnumerable();
                s.Pipeline.ProcessPipesAsync(targetPipes, CancellationToken.None);
            }, state, watchTriggerDelay);

            //IDisposable disposable = ChangeToken.OnChange(() => fileProvider.Watch(state.Input), (s) =>
            //{
               

            //}, state);

            return token;            
        }

        protected class StateInfo
        {
            public string Input { get; set; }
            public IPipeLine Pipeline { get; set; }
            public PipeProcessor PipeContext { get; set; }
        }
    }
}