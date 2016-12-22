using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NetPack.RequireJs;

namespace NetPack.Pipeline
{

    public class StateInfo
    {
        public string Input { get; set; }
        public IPipeLine Pipeline { get; set; }
        public PipeConfiguration PipeConfig { get; set; }
    }

    public class PipelineWatcher : IPipelineWatcher
    {
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private ConcurrentBag<IPipeLine> _pipelines = new ConcurrentBag<IPipeLine>();
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
                    var state = new StateInfo() { Input = include, PipeConfig = pipe, Pipeline = pipeline };
                    WatchChangeToken(state);
                }
            }

            //input.WatchFiles(a =>
            //{
            //    // var coll = (System.Collections.Concurrent.IProducerConsumerCollection<IPipeLine>)_pipelineFlushRequest;
            //    _pipelineFlushRequest.Add(pipeline);
            //});

            EnsureMonitorTaskRunning();
        }

        private void HandleChangeTokenExpired(object state)
        {
            var stateInfo = state as StateInfo;
            IChangeToken expired;
            _activeChangeTokens.TryRemove(stateInfo.Input, out expired);

            // Mark the input as changed.
            var changeTime = DateTime.UtcNow;
            _logger.LogInformation("Changed signalled @ {0} for {1}", changeTime, stateInfo.Input);
            stateInfo.PipeConfig.Input.LastChanged = changeTime;
            // _changedSinceLastTime.Add(stateInfo);
            // re-watch the input as token will have expired.
            WatchChangeToken(stateInfo);
        }

        private IChangeToken WatchChangeToken(StateInfo state)
        {
            IChangeToken changeToken;
            changeToken = _activeChangeTokens.GetOrAdd(state.Input, (key) =>
            {
                var token = state.Pipeline.InputAndOutputFileProvider.Watch(key);
                return token;
            });

            // var pipeWithInclude = new Tuple<PipeConfiguration, string>(pipeConfig, key);
            changeToken.RegisterChangeCallback(HandleChangeTokenExpired, state);
            return changeToken;
        }

        private void EnsureMonitorTaskRunning()
        {
            if (_monitoringTask == null)
            {
                _monitoringTask = Task.Run(async () => await FlushPipelineAsync());
                _logger.LogInformation("Watching..");
            }
        }

        private async Task FlushPipelineAsync()
        {
            while (!_tokenSource.Token.IsCancellationRequested)
            {

                var pipeLines = _pipelines.ToArray();

                // IEnumerable<PipeConfiguration> dirtyPipeLiness = pipeLines.Select(p => p.GetDirtyPipes());
                //foreach (var pipe in pipeLines)
                //{
                //    if(pipe.GetDirtyPipes())
                //    {

                //    }
                //}


                var dirtyPipeLines = pipeLines.Select(p =>
                       new
                       {
                           DirtyPipes = p.GetDirtyPipes(),
                           Pipeline = p
                       }).Where(a => a.DirtyPipes.Any());

                bool hasWork = dirtyPipeLines.Any();
                while (hasWork)
                {

                    List<Task> _tasks = new List<Task>(dirtyPipeLines.Count());

                    foreach (var dirtyPipeline in dirtyPipeLines)
                    {
                        //todo: use proper cancellation token, so we can signal cancellation if more files change whilst processing?
                        _tasks.Add(dirtyPipeline.Pipeline.ProcessPipesAsync(dirtyPipeline.DirtyPipes, CancellationToken.None));
                    }

                    _logger.LogInformation("Processing changed pipes..");
                    await Task.WhenAll(_tasks);

                    // processing pipes may have lead to other pipes becoming dirty, so check again and process those new pipes straight away if so
                    dirtyPipeLines = pipeLines.Select(p =>
                       new
                       {
                           DirtyPipes = p.GetDirtyPipes(),
                           Pipeline = p
                       }).Where(a => a.DirtyPipes.Any());
                    hasWork = dirtyPipeLines.Any();
                }

                // check again in x seconds.
                await Task.Delay(TimeSpan.FromSeconds(2), _tokenSource.Token);
            }

        }
    }
}