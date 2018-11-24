using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Pipeline
{

    public class StateInfo
    {
        public string Input { get; set; }
        public IPipeLine Pipeline { get; set; }
        public PipeProcessor PipeContext { get; set; }
    }

    public class PipelineWatcher : IPipelineWatcher
    {
        private readonly CancellationTokenSource _tokenSource;
        private List<IPipeLine> _pipelines = new List<IPipeLine>();
        private readonly Task _monitoringTask = null;

       // private readonly ConcurrentDictionary<Guid, IDisposable> _activeChangeTokens = new ConcurrentDictionary<Guid, IDisposable>();
        private ILogger<PipelineWatcher> _logger = null;


        public PipelineWatcher(ILogger<PipelineWatcher> logger)
        {
            _logger = logger;
        }

        public void WatchPipeline(IPipeLine pipeline)
        {
            _pipelines.Add(pipeline);
            foreach (PipeProcessor pipe in pipeline.Pipes)
            {
                //var tokens = new List<IChangeToken>();
                foreach (string include in pipe.Input.GetIncludes())
                {
                    StateInfo state = new StateInfo() { Input = include, PipeContext = pipe, Pipeline = pipeline };      
                    var token = WatchInput(state);


                   // tokens.Add(token);
                }
            }
        }

        private IChangeToken WatchInput(StateInfo state)
        {

            Guid key = Guid.NewGuid();
            var fileProvider = state.Pipeline.Context.FileProvider;

            IChangeToken token = fileProvider.Watch(state.Input);

            IDisposable disposable = ChangeToken.OnChange(() => fileProvider.Watch(state.Input), (s) =>
            {
                // Mark the input as changed.
                DateTime changeTime = DateTime.UtcNow;
                _logger.LogInformation("Changed signalled @ {0} for {1}, key: {2}", changeTime, s.Input, key);
                //  s.PipeContext.Input.LastChanged = changeTime;
                var targetPipes = new[] { s.PipeContext }.AsEnumerable();

                s.Pipeline.ProcessPipesAsync(targetPipes, CancellationToken.None);
                // trigger the pipe to process the changed files.
              //  s.PipeContext.ProcessChanges(s.Pipeline);

            }, state);

            return token;


            //IDisposable disposable = token.RegisterChangeCallback((s) =>
            //{
            //    StateInfo stateInfo = s as StateInfo;
            //    // Mark the input as changed.
            //    DateTime changeTime = DateTime.UtcNow;
            //    _logger.LogInformation("Changed signalled @ {0} for {1}", changeTime, stateInfo.Input);
            //    stateInfo.PipeContext.Input.LastChanged = changeTime;

            //    // dispose this delegate and re-watch          
            //    IDisposable removed = null;
            //    int remainingAttempts = 10;

            //    while (removed == null)
            //    {
            //        if (_activeChangeTokens.TryRemove(key, out removed))
            //        {
            //            if (removed != null)
            //            {
            //                removed.Dispose();
            //                WatchInput(stateInfo);
            //                break;
            //            }
            //        }

            //        remainingAttempts = remainingAttempts - 1;
            //        if (remainingAttempts <= 0)
            //        {
            //            throw new Exception("Unable to remove watch handler on " + stateInfo.Input);
            //        }
            //    }

            //    // trigger the pipe to process the changed files.
            //    stateInfo.PipeContext.ProcessChanges(stateInfo.Pipeline);

            //}, state);

            //_activeChangeTokens.AddOrUpdate(key, disposable, (a, b) =>
            //{
            //    return b;
            //});

            //  return disposable;
        }
    }
}