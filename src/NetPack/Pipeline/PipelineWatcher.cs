using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NetPack.Pipeline
{
    public class PipelineWatcher : IPipelineWatcher
    {
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private ConcurrentBag<IPipeLine> _pipelines = new ConcurrentBag<IPipeLine>();
        private Task _monitoringTask = null;
        private ConcurrentBag<IPipeLine> _pipelineFlushRequest = new ConcurrentBag<IPipeLine>();

        private ILogger<PipelineWatcher> _logger = null;

        public PipelineWatcher(ILogger<PipelineWatcher> logger)
        {
            _logger = logger;
        }

        public void WatchPipeline(IPipeLine pipeline)
        {
            _pipelines.Add(pipeline);
            var input = pipeline.Input;
         
            input.WatchFiles(a =>
            {
                var coll = (System.Collections.Concurrent.IProducerConsumerCollection<IPipeLine>)_pipelineFlushRequest;
                coll.TryAdd(pipeline);
            });

            EnsureMonitorTaskRunning();
        }

        private void EnsureMonitorTaskRunning()
        {
            if (_monitoringTask == null)
            {
                _monitoringTask = Task.Run(async () => await FlushPipelineAsync());
            }
        }

        private async Task FlushPipelineAsync()
        {
            while (!_tokenSource.Token.IsCancellationRequested)
            {
                // are there file changes to process?
                if (!_pipelineFlushRequest.IsEmpty)
                {
                    _logger.LogInformation("Flushing pipeline");
                    IPipeLine outPipeline;
                    while (_pipelineFlushRequest.TryTake(out outPipeline))
                    {
                        await outPipeline.FlushAsync(CancellationToken.None);
                    }
                }
                // wait some delay between flushing pipes, as if several files are modified at once,
                // we would rather 
                await Task.Delay(TimeSpan.FromSeconds(2), _tokenSource.Token);
            }

        }
    }
}