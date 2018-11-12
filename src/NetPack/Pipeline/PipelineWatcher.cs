using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly CancellationTokenSource _tokenSource;

        //  private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private List<IPipeLine> _pipelines = new List<IPipeLine>();
        private readonly Task _monitoringTask = null;

        private readonly ConcurrentDictionary<Guid, IDisposable> _activeChangeTokens = new ConcurrentDictionary<Guid, IDisposable>();
        private ILogger<PipelineWatcher> _logger = null;


        public PipelineWatcher(ILogger<PipelineWatcher> logger)
        {
            _logger = logger;
        }

        public void WatchPipeline(IPipeLine pipeline)
        {
            _pipelines.Add(pipeline);
            //  var inputs = pipeline.Pipes.Select(a => a.Input);

            foreach (PipeContext pipe in pipeline.Pipes)
            {
                foreach (string include in pipe.Input.GetIncludes())
                {

                    //input.IncludeList.ForEach((include) =>
                    //{
                    StateInfo state = new StateInfo() { Input = include, PipeContext = pipe, Pipeline = pipeline };
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

        private IDisposable WatchChangeToken(StateInfo state)
        {

            Guid key = Guid.NewGuid();
            IChangeToken token = state.Pipeline.InputAndGeneratedFileProvider.Watch(state.Input);

            IDisposable disposable = token.RegisterChangeCallback((s) =>
            {
                StateInfo stateInfo = s as StateInfo;
                // Mark the input as changed.
                DateTime changeTime = DateTime.UtcNow;
                _logger.LogInformation("Changed signalled @ {0} for {1}", changeTime, stateInfo.Input);
                stateInfo.PipeContext.Input.LastChanged = changeTime;

                // dispose this delegate and re-watch          
                IDisposable removed = null;
                while (removed == null)
                {
                    _activeChangeTokens.TryRemove(key, out removed);
                    if(removed != null)
                    {
                        removed.Dispose();
                        var newHandler = WatchChangeToken(stateInfo);
                    }                    
                }

                stateInfo.PipeContext.ProcessChanges(stateInfo.Pipeline);

            }, state);

            _activeChangeTokens.AddOrUpdate(key, disposable, (a,b)=> {
                return b;
            });
            //var changeToken = _activeChangeTokens.Add(state.Input, (key) =>
            //{

            //    return disposable;
            //});

            // var pipeWithInclude = new Tuple<PipeConfiguration, string>(pipeConfig, key);

            return disposable;
        }

        //private void HandleChangeTokenExpired(object state)
        //{
        //    var stateInfo = state as StateInfo;
        //    IChangeToken expired;

        //    _activeChangeTokens.TryRemove(stateInfo.Input, out expired);

        //    // Mark the input as changed.
        //    var changeTime = DateTime.UtcNow;
        //    _logger.LogInformation("Changed signalled @ {0} for {1}", changeTime, stateInfo.Input);
        //    stateInfo.PipeContext.Input.LastChanged = changeTime;
        //    // _changedSinceLastTime.Add(stateInfo);
        //    // re-watch the input as token will have expired.
        //    WatchChangeToken(stateInfo);
        //    stateInfo.PipeContext.ProcessChanges(stateInfo.Pipeline);          

        //}




    }
}