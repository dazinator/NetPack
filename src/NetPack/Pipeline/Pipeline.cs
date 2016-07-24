using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetPack.Pipes;

namespace NetPack.Pipeline
{
    public class Pipeline : IPipeLine
    {
        public static TimeSpan DefaultFlushTimeout = new TimeSpan(0, 5, 0);

        public Pipeline(PipelineInput input, List<IPipe> pipes, bool watch)
        {
            Input = input;
            Pipes = pipes;

            if (watch)
            {
                // when files change, re-flush the pipeline. (in other words, process all the input
                // files of the pipeline again.
                input.WatchFiles(async a =>
                {
                    // leave some delay?
                    await this.FlushAsync();
                });
            }

            IsWatching = watch;
            HasFlushed = false;
        }

        public PipelineInput Input { get; set; }

        public PipelineOutput Output { get; set; }

        public List<IPipe> Pipes { get; set; }

        /// <summary>
        /// Processes the current input through the pipes in the pipeline, and returns the output of the pipeline.
        /// </summary>
        /// <returns></returns>
        public async Task<PipelineOutput> FlushAsync()
        {
            
            var context = new PipelineContext(Input.Files);

            foreach (var pipe in Pipes)
            {
                await pipe.ProcessAsync(context);
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

        ///// <summary>
        ///// Processes the current input through the pipes in the pipeline, and returns the output of the pipeline.
        ///// </summary>
        ///// <returns></returns>
        //public PipelineOutput Flush(TimeSpan? timeout = null)
        //{
        //    var result = Task.Run(FlushAsync);
        //    result.Wait(timeout ?? DefaultFlushTimeout);

        //    if (result.IsFaulted)
        //    {
        //        throw result.Exception;
        //    }

        //    //var context = new PipelineContext(Input.Files);

        //    //foreach (var pipe in Pipes)
        //    //{
        //    //    var flushTimeout = timeout ?? DefaultFlushTimeout;
        //    //    pipe.ProcessAsync(context).Wait(flushTimeout);
        //    //    context.PrepareNextInputs();
        //    //}

        //    //// whatever is currently the inputs for the "next" pipe (even though we dont have any more pipe)
        //    //// is actually the output we want to return from the pipe.
        //    //var output = new PipelineOutput(context.InputFiles);

        //    //FlushCount = FlushCount + 1;
        //    //HasFlushed = true;

        //    //return output;


        //}

        public bool IsWatching { get; set; }

        public int FlushCount { get; set; }

        public bool HasFlushed { get; set; }



    }
}