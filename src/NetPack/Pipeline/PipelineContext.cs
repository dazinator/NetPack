using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.AspNetCore.Http;
using Polly;
using System.IO;

namespace NetPack.Pipeline
{

    public class PipelineContext : IPipelineContext
    {

        public PipelineContext(
            IFileProvider fileProvider,
            IDirectory sourceOutput) : this(
            fileProvider,
            sourceOutput, new InMemoryDirectory())
        {

        }

        public PipelineContext(
            IFileProvider fileProvider,
            IDirectory sourceOutput, IDirectory directory) : this(
                fileProvider,
                sourceOutput, directory, string.Empty)
        {

        }

        public Policy Policy { get; set; }

        public PipelineContext(
            IFileProvider fileProvider,
            IDirectory sourceOutput, IDirectory directory, string baseRequestPath)
        {
            FileProvider = fileProvider;
            SourcesOutput = sourceOutput;
            GeneratedOutput = directory;
            BaseRequestPath = baseRequestPath;
            //Input = input;

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

        public PathString BaseRequestPath { get; }

        public PathString GetRequestPath(string directory, IFileInfo fileInfo)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return BaseRequestPath.Add("/" + fileInfo.Name);
            }

            if (!directory.StartsWith("/"))
            {
                return BaseRequestPath.Add($"/{directory}").Add("/" + fileInfo.Name);
            }

            return BaseRequestPath.Add(directory).Add("/" + fileInfo.Name);
            //return BaseRequestPath.Add(directory).Add(fileInfo.Name);
        }

        public IDirectory GeneratedOutput { get; set; }

        public IDirectory SourcesOutput { get; set; }

        public IFileProvider FileProvider { get; set; }

        public PipeContext PipeContext { get; set; }

        public async Task Apply(PipeContext pipe, CancellationToken cancellationToken)
        {

            try
            {
                PipeContext = pipe;
                PipeContext.LastProcessStartTime = DateTime.UtcNow;
                PipeContext.SetInputFiles(FileProvider);

                if (PipeContext.HasChanges)
                {
                    PipeContext.IsProcessing = true;
                    await Policy.ExecuteAsync(ct => PipeContext.Pipe.ProcessAsync(this, ct), cancellationToken);
                }
            }
            catch (Exception e)
            {
                //todo: log exception..
                throw;
            }
            finally
            {
                PipeContext.LastProcessedEndTime = DateTime.UtcNow;
                PipeContext.IsProcessing = false;
            }

        }


        /// <summary>
        /// Any file added here will be able to be served up to the browser, but will not trigger
        /// any additional processing, as it's assumed to be a source file which is not modified, and for which some generated 
        /// processed file was produced. Usually you want this method if you need to expose the original source file because source maps are
        /// being used.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileInfo"></param>
        public void AddSourceOutput(string directory, IFileInfo fileInfo)
        {
            this.SourcesOutput.AddOrUpdateFile(directory, fileInfo);
            //  throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the generated file as an output of the pipeline.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="file"></param>
        /// <param name="excludeFromInput">Whether to add the output ile to the exclude list of this pipelines input. True by default, prevents the pipeline from processing a file which it has generated.</param>
        public void AddGeneratedOutput(string directory, IFileInfo file)
        {
            //if (excludeFromInput)
            //{
            //    //to-do - check for concurrency?
            //    Input.AddExclude($"{directory}/{file.Name}");
            //}
            GeneratedOutput.AddOrUpdateFile(directory, file);
            // return new FileWithDirectory(directory, file);
            //  Output.AddFile(directory, info);
        }



    }
}