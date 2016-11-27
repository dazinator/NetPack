using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipes;
using NetPack.Requirements;
using Polly;

namespace NetPack.Pipeline
{
    public class Pipeline : IPipeLine
    {


        public static TimeSpan DefaultFlushTimeout = new TimeSpan(0, 5, 0);

        public Pipeline(IFileProvider fileProvider, IDirectory directory, List<PipeConfiguration> pipes, List<IRequirement> requirements)
        {
            FileProvider = fileProvider;
            Pipes = pipes;
            Requirements = requirements;
            //  IsWatching = watch;
            HasFlushed = false;
            Output = directory;
            // FileProvider = fileProvider;

        }

        public IFileProvider FileProvider { get; set; }

        public IDirectory Output { get; set; }

        public List<PipeConfiguration> Pipes { get; set; }

        public List<IRequirement> Requirements { get; set; }

        private string _requestPath;
        public string RequestPath
        {
            get { return _requestPath; }
            set
            {
                _requestPath = value;
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
                FlushAsync(CancellationToken.None).Wait(DefaultFlushTimeout);
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
        public async Task FlushAsync(CancellationToken cancelationToken)
        {

            var context = new PipelineContext(this.FileProvider, this.Output, this.RequestPath);

            try
            {

                var policy = Policy.Handle<IOException>()
                      .WaitAndRetryAsync(new[]
    {
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2),
    TimeSpan.FromSeconds(3)
    }, (exception, timeSpan) =>
    {
        // TODO: Log exception    
    });

                foreach (var pipe in Pipes)
                {
                    var inputs = pipe.Input;
                    IFileInfo[] inputFiles = GetInputFiles(inputs, FileProvider);
                    await policy.ExecuteAsync(ct => pipe.Pipe.ProcessAsync(context, inputFiles, ct), cancelationToken);
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

        private IFileInfo[] GetInputFiles(PipelineInput inputs, IFileProvider fileProvider)
        {
            var results = new List<IFileInfo>();
            foreach (var input in inputs.IncludeList)
            {
                var files = fileProvider.Search(input);
                // check if file already present? Multiple input patterns can match the same files.
                foreach (var file in files)
                {
                    if (!results.Contains(file))
                    {
                        results.Add(file);
                    }
                }
                
            }
            return results.ToArray();
        }

        //public bool IsWatching { get; set; }

        public int FlushCount { get; set; }

        public bool HasFlushed { get; set; }






    }
}