using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipes;
using NetPack.Requirements;
using Polly;

namespace NetPack.Pipeline
{
    public class Pipeline : IPipeLine
    {


        public static TimeSpan DefaultFlushTimeout = new TimeSpan(0, 5, 0);


        public Pipeline(PipelineInput input, List<IPipe> pipes, List<IRequirement> requirements)
        {
            Input = input;
            Pipes = pipes;
            Requirements = requirements;

            //if (watch)
            //{
            //    // when files change, re-flush the pipeline. (in other words, process all the input
            //    // files of the pipeline again.
            //    input.WatchFiles(a =>
            //    {
            //        _changedFiles.Enqueue(a);
            //        //// leave some delay?
            //        //if (HasFlushed) // only bother doing when initialised, cos we do a flush on initialise.
            //        //{
            //        //    await this.FlushAsync();
            //        //}

            //    });
            //}

            //  IsWatching = watch;
            HasFlushed = false;
           // FileProvider = fileProvider;

        }

        public PipelineInput Input { get; set; }

        public PipelineOutput Output { get; set; }

        public List<IPipe> Pipes { get; set; }

        public List<IRequirement> Requirements { get; set; }

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
        /// Processes the current input through the pipes in the pipeline, and returns the output of the pipeline.
        /// </summary>
        /// <returns></returns>
        public async Task<PipelineOutput> FlushAsync(CancellationToken cancelationToken)
        {

            var context = new PipelineContext(Input.Files);
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
                    await policy.ExecuteAsync(ct => pipe.ProcessAsync(context, ct), cancelationToken);
                    //  await pipe.ProcessAsync(context);
                    context.PrepareNextInputs();
                }

                // whatever is currently the inputs for the "next" pipe (even though we dont have any more pipe)
                // is actually the output we want to return from the pipe.
                var output = new PipelineOutput(context.InputFiles);
                Output = output;

                FlushCount = FlushCount + 1;
                HasFlushed = true;

                return output;
            }
            catch (Exception e)
            {
                // retry?
                throw;
            }

        }

        //public bool IsWatching { get; set; }

        public int FlushCount { get; set; }

        public bool HasFlushed { get; set; }

        public IFileProvider FileProvider { get; set; }


    }
}