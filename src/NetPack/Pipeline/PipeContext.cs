using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NetPack.FileLocking;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Pipeline
{

    public class PipeContext
    {

        private readonly object _lock = new object();
        private CancellationTokenSource _tokenSource;
        private ILogger<PipeContext> _logger;


        public PipeContext(PipelineInput input, IPipe pipe, ILogger<PipeContext> logger)
        {
            Input = input;
            Pipe = pipe;
            _logger = logger;
            Policy = Policy.Handle<IOException>()
                  .WaitAndRetryAsync(new[] {
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
            bool isDirty = (Input.LastChanged > LastProcessStartTime);
            // isDirty = isDirty && Input.LastChanged <= LastProcessedEndTime; // inputs were changed in between last processing start and end time.
            //isDirty = isDirty || Input.LastChanged > LastProcessedEndTime; // or inputs have been changed since last processing time ended.

            return isDirty;
        }

        /// <summary>
        /// Returns the previous files that were processed.
        /// </summary>
        private FileWithDirectory[] PreviousInputFiles { get; set; }

        /// <summary>
        /// Returns the previous files that were output.
        /// </summary>
        private HashSet<Tuple<PathString, IFileInfo>> PreviousOutputFiles { get; set; }

        /// <summary>
        /// Returns the previous sources that were output.
        /// </summary>
        private HashSet<Tuple<PathString, IFileInfo>> PreviousSources { get; set; }

        /// <summary>
        ///   /// <summary>
        /// Returns the version of the file that was processed last time.
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        public FileWithDirectory GetPreviousVersionOfInputFile(FileWithDirectory fileWithDirectory)
        {
            if (PreviousInputFiles == null)
            {
                return null;
            }

            foreach (FileWithDirectory item in PreviousInputFiles)
            {
                if (item.UrlPath == fileWithDirectory.UrlPath)
                {
                    return item;
                }
            }

            return null;
        }

        public bool IsInputDifferentFromLastTime(FileWithDirectory fileWithDirectory)
        {
            FileWithDirectory oldFile = GetPreviousVersionOfInputFile(fileWithDirectory);
            if (oldFile == null)
            {
                // We don't have a previous version of this file, so the file must be a new version.
                return true;
            }

            // Return whether this version of the file has a gretaer modified date than the last version of this file.
            bool changed = fileWithDirectory.FileInfo.LastModified > oldFile.FileInfo.LastModified;
            return changed;

        }

        /// <summary>
        /// Returns all the files that are detected as inputs for processing.
        /// </summary>
        public FileWithDirectory[] InputFiles { get; set; }

        /// <summary>
        /// Returns all the files that are detected as outputs from processing.
        /// </summary>
        public HashSet<Tuple<PathString, IFileInfo>> OutputFiles { get; set; }

        /// <summary>
        /// These are outputs from the pipeline but represent source / unchanged files.
        /// </summary>
        public HashSet<Tuple<PathString, IFileInfo>> Sources { get; set; }

        public IEnumerable<FileWithDirectory> GetChangedInputFiles()
        {
            if (InputFiles == null)
            {
                yield break;
            }
            else
            {
                foreach (FileWithDirectory item in InputFiles)
                {
                    if (IsInputDifferentFromLastTime(item))
                    {
                        yield return item;
                    }
                }
            }

        }

        public bool HasChanges { get; private set; }

        public void SetInputFiles(IFileProvider fileProvider)
        {
            SetInput(fileProvider.GetFiles(Input));
            // ExcludeFiles = FileProvider.GetFiles(input.e)
        }

        private void SetInput(FileWithDirectory[] newInputs)
        {
            // grab the input files from the file provider.
            PreviousInputFiles = InputFiles;
            PreviousOutputFiles = OutputFiles;
            PreviousSources = Sources;

            OutputFiles = OutputFiles ?? new HashSet<Tuple<PathString, IFileInfo>>();
            OutputFiles.Clear();

            Sources = Sources ?? new HashSet<Tuple<PathString, IFileInfo>>();
            Sources.Clear();

            InputFiles = newInputs;
            HasChanges = false;
            List<FileWithDirectory> changedInputs = new List<FileWithDirectory>(InputFiles.Length);
            foreach (FileWithDirectory item in InputFiles)
            {
                if (IsInputDifferentFromLastTime(item))
                {
                    HasChanges = true;
                    break;
                }
            }

        }

        protected IFileBlocker Blocker { get; private set; }

        public void AddBlock(PathString path)
        {
            PathString outputPath = PipelineContext.BaseRequestPath.Add(path);
            Blocker.AddBlock(outputPath);
        }

        internal async Task ProcessChanges(IPipeLine parentPipeline)
        {

            if (IsDirty())
            {
                try
                {
                    CancellationToken token = ResetWorkToken();
                    _logger.LogInformation("Processing changes..");

                    SetInputFiles(parentPipeline.InputAndGeneratedFileProvider);
                    // lock previous outputs
                    using (Blocker = new FileLocker())
                    {

                        //  fileBlocker.AddBlocks(PreviousOutputFiles);
                        //  fileBlocker.AddBlocks(PreviousOutputFiles);

                        LastProcessStartTime = DateTime.UtcNow;

                        if (HasChanges)
                        {
                            IsProcessing = true;
                            PipelineContext = parentPipeline.Context;
                            await Policy.ExecuteAsync(ct => Pipe.ProcessAsync(this, ct), token);
                            // flush outputs from succesfully processed pipe into pipeline.
                            FlushOutputs(PipelineContext);
                        }
                    }


                }
                catch (Exception ex)
                {
                    // should log..
                    // for now exit while so that we try again.
                    _logger.LogError(new EventId(0), ex, "Netpack was unable to process files.");

                }
                finally
                {
                    LastProcessedEndTime = DateTime.UtcNow;
                    IsProcessing = false;
                }

            }

        }

        private void FlushOutputs(IPipelineContext pipelineContext)
        {
            foreach (Tuple<PathString, IFileInfo> item in OutputFiles)
            {
                pipelineContext.AddGeneratedOutput(item.Item1, item.Item2);
            }

            foreach (Tuple<PathString, IFileInfo> item in Sources)
            {
                PipelineContext.AddSourceOutput(item.Item1, item.Item2);
            }
        }

        public void AddOutput(PathString directory, IFileInfo file)
        {
            // var item = new FileWithDirectory() { Directory = directory, FileInfo = fileInfo };
            var pathAndFile = Tuple.Create<PathString, IFileInfo>(directory, file);
            OutputFiles.Add(pathAndFile);
        }

        /// <summary>
        /// Source files added as output do not trigger any onward processing, but can be served up my netpack's IFileProvider.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileInfo"></param>
        public void AddSource(PathString directory, IFileInfo file)
        {
            var pathAndFile = Tuple.Create<PathString, IFileInfo>(directory, file);
            Sources.Add(pathAndFile);            
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

        public IPipelineContext PipelineContext { get; set; }

    }
}