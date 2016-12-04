using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using NetPack;
using NetPack.Pipes;
using NetPack.Requirements;
using Polly;
using NetPack.Pipeline;

namespace NetPack.Pipeline
{

    public class Pipeline : IPipeLine
    {


        public static TimeSpan DefaultFlushTimeout = new TimeSpan(0, 5, 0);

        public Pipeline(IFileProvider environmentFileProvider, List<PipeConfiguration> pipes, List<IRequirement> requirements, string baseRequestPath = null, IDirectory directory = null)
        {
            EnvironmentFileProvider = environmentFileProvider;
            Pipes = pipes;
            Requirements = requirements;
            BaseRequestPath = baseRequestPath;
            //  IsWatching = watch;
            HasFlushed = false;
            Directory = directory ?? new InMemoryDirectory();
            OutputFileProvider = new InMemoryFileProvider(Directory);
            InputAndOutputFileProvider = new CompositeFileProvider(EnvironmentFileProvider, OutputFileProvider);
        }

        /// <summary>
        /// Provides access to files that need to be processed from the environment. 
        /// This does not include access to new files that are produced only as a result of pipeline processing.
        /// </summary>
        public IFileProvider EnvironmentFileProvider { get; set; }

        /// <summary>
        /// Provides access too all output files only. These are files that were output from pipes that processed inputs.
        /// </summary>
        public IFileProvider OutputFileProvider { get; set; }
        /// <summary>
        /// Provides access to all files, this means files accessed from the environment, plus files that are produced only as a product of pipeline processing.
        /// </summary>
        public IFileProvider InputAndOutputFileProvider { get; set; }

        public IDirectory Directory { get; set; }

        public List<PipeConfiguration> Pipes { get; set; }

        public List<IRequirement> Requirements { get; set; }

        private string _baseRequestPath;
        public string BaseRequestPath
        {
            get { return _baseRequestPath; }
            set
            {
                _baseRequestPath = value;
                //Output.SetRequestPaths(_requestPath);
            }
        }

        public void Initialise()
        {
            // run checks for requirements.
            EnsureRequirements();
            // Trigger the pipeline to be flushed if it hasn't already.
            // we want to block becausewe dont want the app to finish starting
            // before all assets have been processed..
            if (!HasFlushed)
            {
                ProcessAsync(CancellationToken.None).Wait(DefaultFlushTimeout);
                // await pipeline.FlushAsync();
            }
        }

        private void EnsureRequirements()
        {
            foreach (var requirement in Requirements)
            {
                requirement.Check();
            }
        }

        /// <summary>
        /// Processes all pipes.
        /// </summary>
        /// <returns></returns>
        public Task ProcessAsync(CancellationToken cancelationToken)
        {
            return ProcessPipesAsync(Pipes, cancelationToken);
        }

        public async Task ProcessPipesAsync(IEnumerable<PipeConfiguration> pipes, CancellationToken cancellationToken)
        {
            var context = new PipelineContext(this.InputAndOutputFileProvider, this.Directory, this.BaseRequestPath);

            try
            {
                Policy policy = null;
                //            // If debugger attach, use a policy that retries after 60 minutes instead of 
                //            if (Debugger.IsAttached)
                //            {
                //                policy = Policy.Handle<IOException>()
                //                   .WaitAndRetryAsync(new[]
                // {
                //TimeSpan.FromMinutes(60)
                // }, (exception, timeSpan) =>
                // {
                //     // TODO: Log exception    
                // });

                //            }

                policy = Policy.Handle<IOException>()
                      .WaitAndRetryAsync(new[]
    {
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2),
    TimeSpan.FromSeconds(3)
    }, (exception, timeSpan) =>
    {
        // TODO: Log exception    
    });

                foreach (var pipe in pipes)
                {
                    try
                    {
                        pipe.LastProcessStartTime = DateTime.UtcNow;
                        pipe.IsProcessing = true;
                        var input = pipe.Input;
                        var inputFiles = input.GetFiles(context.FileProvider);
                        await policy.ExecuteAsync(ct => pipe.Pipe.ProcessAsync(context, inputFiles, ct), cancellationToken);
                        pipe.LastProcessedEndTime = DateTime.UtcNow;
                    }
                    catch (Exception e)
                    {
                        //todo: log exception..
                        throw;
                    }
                    finally
                    {
                        pipe.IsProcessing = false;
                    }

                    //  await pipe.ProcessAsync(context);
                }

                FlushCount = FlushCount + 1;
                HasFlushed = true;
            }
            catch (Exception e)
            {
                // retry?
                throw;
            }
        }


        //private FileWithDirectory[] GetInputFiles(PipelineInput inputs, IFileProvider fileProvider)
        //{
        //    var results = new Dictionary<string, FileWithDirectory>();
        //    foreach (var input in inputs.IncludeList)
        //    {
        //        var files = fileProvider.Search(input);
        //        // check if file already present? Multiple input patterns can match the same files.
        //        foreach (var file in files)
        //        {
        //            var path = $"{file.Item1}/{file.Item2.Name}";
        //            if (!results.ContainsKey(path))
        //            {
        //                var item = new FileWithDirectory() { Directory = file.Item1, FileInfo = file.Item2 };
        //                results.Add(path, item);
        //            }
        //        }

        //    }
        //    return results.Values.ToArray();
        //}

        public IEnumerable<PipeConfiguration> GetDirtyPipes()
        {
            return Pipes.Where(a => a.IsDirty());
        }



        //public bool IsWatching { get; set; }

        public int FlushCount { get; set; }

        public bool HasFlushed { get; set; }


        //public IEnumerable<IChangeToken> WatchInputs()
        //{

        //    var inputs = Pipes.Select(a => a.Input);
        //    List<IChangeToken> _tokens;

        //    foreach (var input in inputs)
        //    {
        //        input.IncludeList.ForEach((include) =>
        //        {
        //            var token = FileProvider.Watch(include);
        //            yield return token;
        //        });


        //    }

        //    //input.IncludeList.ForEach((include) =>
        //    //{
        //    //    var changeToken = pipeline.FileProvider.Watch(include);
        //    //});
        //}


    }


}


