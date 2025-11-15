using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Polly.Retry;

namespace NetPack.Pipeline
{
    public class PipeProcessor
    {

        private readonly object _lock = new object();
        private CancellationTokenSource _tokenSource;
        private ILogger<PipeProcessor> _logger;
        private PipeState _previousState = null;

        public PipeProcessor(PipeInput input, IPipe pipe, ILogger<PipeProcessor> logger)
        {
            Input = input;
            Pipe = pipe;
            _logger = logger;
            RetryPolicy = Policy.Handle<IOException>()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                }, onRetry: (ex, timeSpan, retryAttempt, context) =>
                {
                    // TODO: Log the exception
                    Console.WriteLine($"Retry {retryAttempt} after {timeSpan.Seconds} seconds due to: {ex.Message}");
                });
        }

        public IPipe Pipe { get; set; }
        public PipeInput Input { get; set; }
        public DateTime LastProcessedEndTime { get; set; } = DateTime.MinValue.ToUniversalTime();
        public DateTime LastProcessStartTime { get; set; } = DateTime.MinValue.ToUniversalTime();
        public bool IsProcessing { get; set; }

        public AsyncRetryPolicy  RetryPolicy { get; set; }

        /// <summary>
        /// Returns true if the input specification for the pipe has been changed since the pipe was last processed. 
        /// For example, if the input for the pipe was "**/foo.js" and the pipe hasn't been processed this would return true, or if the pipe was processed
        /// but then the input specification was changed from "**/foo.js" to "/bar.js" then it would also return true.
        /// If the input specification for the pipe hasn't changed since it was last processed this will return false, however that doesn't mean that the input files 
        /// themselves have not changed, they need to be checked seperately.
        /// </summary>
        /// <returns></returns>
        internal bool IsUninitialised()
        {
            bool isDirty = (Input.LastChanged > LastProcessStartTime);
            return isDirty;
        }

        public PipeState LoadState(IFileProvider fileProvider)
        {
            FileWithDirectory[] inputFiles = fileProvider.GetFiles(Input);
            return new PipeState(inputFiles, fileProvider);
        }

        internal async Task ProcessChanges(IPipeLine parentPipeline)
        {

            CancellationToken token = ResetWorkToken();

            try
            {

                _logger.LogInformation("Detecting changes for pipe: {pipe}", Pipe.Name);

                PipeState state = LoadState(parentPipeline.Context.FileProvider);
                bool hasChanges = _previousState == null || state.HasChanged(_previousState);

                if (!hasChanges)
                {
                    _logger.LogInformation("No changes for pipe: {pipe}", Pipe.Name);
                    return;
                }

                _logger.LogInformation("Processing changes for pipe: {pipe}", Pipe.Name);

                LastProcessStartTime = DateTime.UtcNow;
                IsProcessing = true;

                // block serving up requests to files that were previous known outputs from this pipe..
                //using (Blocker = new FileLocker())
                //{
                // todo: concurrent access to OutputFiles might be a problem here..
                //if (_previousState != null && _previousState.OutputFiles.Count > 0)
                //{
                //    //foreach (Tuple<PathString, IFileInfo> item in _previousState.OutputFiles)
                //    //{
                //    //    AddBlock(item.Item1);
                //    //}
                //}


                await RetryPolicy.ExecuteAsync(ct => Pipe.ProcessAsync(state, ct), token);
                // flush outputs from succesfully processed pipe into pipeline.
                if (!token.IsCancellationRequested)
                {
                    SaveState(parentPipeline.Context, state);
                }
                // }
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    LastProcessedEndTime = DateTime.UtcNow;
                }

                IsProcessing = false;
            }
        }

        private void SaveState(IPipelineContext pipelineContext, PipeState state)
        {
            foreach (Tuple<PathString, IFileInfo> item in state.OutputFiles)
            {
                pipelineContext.AddGeneratedOutput(item.Item1, item.Item2);
            }

            foreach (Tuple<PathString, IFileInfo> item in state.SourceFiles)
            {
                pipelineContext.AddSourceOutput(item.Item1, item.Item2);
            }

            _previousState = state;
        }

        /// <summary>
        /// Cancels any existing cancellation token for work in progress, and returns a new cancellation token.
        /// </summary>
        /// <returns></returns>
        private CancellationToken ResetWorkToken()
        {
            lock (_lock)
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Cancel();
                    _tokenSource.Dispose();
                    _tokenSource = new CancellationTokenSource();
                    return _tokenSource.Token;
                }
                else
                {
                    _tokenSource = new CancellationTokenSource();
                    return _tokenSource.Token;
                }
                //    _hasWork = true;



            }
        }

    }
}