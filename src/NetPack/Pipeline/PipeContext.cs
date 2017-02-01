using System;
using NetPack.RequireJs;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using System.Threading;
using Microsoft.Extensions.Logging;
using Polly;
using System.IO;
using System.Threading.Tasks;

namespace NetPack.Pipeline
{

    public class PipeContext
    {

        public PipeContext(PipelineInput input, IPipe pipe, ILogger<PipeContext> logger)
        {
            Input = input;
            Pipe = pipe;
            _logger = logger;

            Policy = Policy.Handle<IOException>()
                  .WaitAndRetryAsync(new[]
{
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2),
    TimeSpan.FromSeconds(3)
}, (exception, timeSpan) =>
{
    // TODO: Log exception    
});
        }



        public IPipe Pipe { get; set; }
        public PipelineInput Input { get; set; }
        public DateTime LastProcessedEndTime { get; set; } = DateTime.MinValue.ToUniversalTime();
        public DateTime LastProcessStartTime { get; set; } = DateTime.MinValue.ToUniversalTime();
        public bool IsProcessing { get; set; }

        public Policy Policy { get; set; }

        /// <summary>
        /// Returns true if the pipe has updated inputs that need to be processed.
        /// </summary>
        /// <returns></returns>
        internal bool IsDirty()
        {
            var isDirty = (Input.LastChanged > LastProcessStartTime);
            // isDirty = isDirty && Input.LastChanged <= LastProcessedEndTime; // inputs were changed in between last processing start and end time.
            //isDirty = isDirty || Input.LastChanged > LastProcessedEndTime; // or inputs have been changed since last processing time ended.

            return isDirty;
        }

        /// <summary>
        /// Returns the previous files that were processed.
        /// </summary>
        FileWithDirectory[] PreviousInputFiles { get; set; }

        /// <summary>
        ///   /// <summary>
        /// Returns the version of the file that was processed last time.
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        public FileWithDirectory GetPreviousVersionOfFile(FileWithDirectory fileWithDirectory)
        {
            if (PreviousInputFiles == null)
            {
                return null;
            }

            foreach (var item in PreviousInputFiles)
            {
                if (item.FileSubPath == fileWithDirectory.FileSubPath)
                {
                    return item;
                }
            }

            return null;
        }

        public bool IsDifferentFromLastTime(FileWithDirectory fileWithDirectory)
        {
            var oldFile = GetPreviousVersionOfFile(fileWithDirectory);
            if (oldFile == null)
            {
                // We don't have a previous version of this file, so the file must be a new version.
                return true;
            }

            // Return whether this version of the file has a gretaer modified date than the last version of this file.
            var changed = fileWithDirectory.FileInfo.LastModified > oldFile.FileInfo.LastModified;
            return changed;

        }

        /// <summary>
        /// Returns all the files that are detected as inputs for processing.
        /// </summary>
        public FileWithDirectory[] InputFiles { get; set; }

        public IEnumerable<FileWithDirectory> GetChangedInputFiles()
        {
            if (InputFiles == null)
            {
                yield break;
            }
            else
            {
                foreach (var item in InputFiles)
                {
                    if (IsDifferentFromLastTime(item))
                    {
                        yield return item;
                    }
                }
            }

        }

        public bool HasChanges { get; private set; }

        public void SetInputFiles(IFileProvider fileProvider)
        {
            SetInput(fileProvider.GetFiles(this.Input));
            // ExcludeFiles = FileProvider.GetFiles(input.e)
        }

        public Action<PipeContext> OnUpdateRequestLocks { get; set; }


        private void SetInput(FileWithDirectory[] input)
        {
            // grab the input files from the file provider.
            PreviousInputFiles = InputFiles;
            InputFiles = input;
            HasChanges = false;
            var changedInputs = new List<FileWithDirectory>(InputFiles.Length);
            foreach (var item in InputFiles)
            {
                if (IsDifferentFromLastTime(item))
                {
                    HasChanges = true;
                    break;
                }
            }

        }

        internal async Task ProcessChanges(IPipeLine parentPipeline)
        {

            if (IsDirty())
            {
                try
                {
                    var token = ResetWorkToken();
                    _logger.LogInformation("Processing changes..");

                    //todo: also populate expected generated file paths for request locking.
                    SetInputFiles(parentPipeline.InputAndGeneratedFileProvider);

                    LastProcessStartTime = DateTime.UtcNow;

                    if (HasChanges)
                    {
                        IsProcessing = true;
                        await Policy.ExecuteAsync(ct => Pipe.ProcessAsync(parentPipeline.Context, ct), token);
                    }
                }
                catch (Exception ex)
                {
                    // should log..
                    // for now exit while so that we try again.
                    _logger.LogError(new EventId(0), ex, "Netpack was unable to process files.");

                }
            }

        }

        private object _lock = new object();
        private CancellationTokenSource _tokenSource;
        private ILogger<PipeContext> _logger;

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

        public List<string> OutputFilesForRequestLocks { get; set; }

    }
}