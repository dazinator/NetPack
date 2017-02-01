using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace NetPack.Pipeline
{

    public class StateInfo
    {
        public string Input { get; set; }
        public IPipeLine Pipeline { get; set; }
        public PipeContext PipeContext { get; set; }
    }

    public class PipelineWatcher : IPipelineWatcher
    {
        private CancellationTokenSource _tokenSource;

        //  private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private List<IPipeLine> _pipelines = new List<IPipeLine>();
        private Task _monitoringTask = null;

        private readonly ConcurrentDictionary<string, IChangeToken> _activeChangeTokens = new ConcurrentDictionary<string, IChangeToken>();
        private ILogger<PipelineWatcher> _logger = null;


        public PipelineWatcher(ILogger<PipelineWatcher> logger)
        {
            _logger = logger;
        }

        public void WatchPipeline(IPipeLine pipeline)
        {
            _pipelines.Add(pipeline);
            //  var inputs = pipeline.Pipes.Select(a => a.Input);

            foreach (var pipe in pipeline.Pipes)
            {
                foreach (var include in pipe.Input.GetIncludes())
                {

                    //input.IncludeList.ForEach((include) =>
                    //{
                    var state = new StateInfo() { Input = include, PipeContext = pipe, Pipeline = pipeline };
                    WatchChangeToken(state);
                }
            }

            //input.WatchFiles(a =>
            //{
            //    // var coll = (System.Collections.Concurrent.IProducerConsumerCollection<IPipeLine>)_pipelineFlushRequest;
            //    _pipelineFlushRequest.Add(pipeline);
            //});

            //  EnsureMonitorTaskRunning();
        }

        private void HandleChangeTokenExpired(object state)
        {
            var stateInfo = state as StateInfo;
            IChangeToken expired;
            _activeChangeTokens.TryRemove(stateInfo.Input, out expired);

            // Mark the input as changed.
            var changeTime = DateTime.UtcNow;
            _logger.LogInformation("Changed signalled @ {0} for {1}", changeTime, stateInfo.Input);
            stateInfo.PipeContext.Input.LastChanged = changeTime;
            // _changedSinceLastTime.Add(stateInfo);
            // re-watch the input as token will have expired.
            WatchChangeToken(stateInfo);
            stateInfo.PipeContext.ProcessChanges(stateInfo.Pipeline);          

        }


        private IChangeToken WatchChangeToken(StateInfo state)
        {
            IChangeToken changeToken;
            changeToken = _activeChangeTokens.GetOrAdd(state.Input, (key) =>
            {
                var token = state.Pipeline.InputAndGeneratedFileProvider.Watch(key);
                return token;
            });

            // var pipeWithInclude = new Tuple<PipeConfiguration, string>(pipeConfig, key);
            changeToken.RegisterChangeCallback(HandleChangeTokenExpired, state);
            return changeToken;
        }
     
    }
}