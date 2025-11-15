using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.IO;
using Dazinator.Extensions.FileProviders.InMemory.Directory;
using Polly;
using Polly.Retry;

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
                sourceOutput, directory, "/")
        {

        }

        public AsyncRetryPolicy  RetryPolicy { get; set; }

        public PipelineContext(
            IFileProvider fileProvider,
            IDirectory sourceOutput,
            IDirectory directory,
            string baseRequestPath)
        {
            FileProvider = fileProvider;
            SourcesOutput = sourceOutput;
            GeneratedOutput = directory;
            BaseRequestPath = baseRequestPath;
            //Input = input;
            RetryPolicy = Policy.Handle<IOException>()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                }, onRetry: (ex, timeSpan, retryAttempt, context) =>
                {
                    // Log the exception
                    Console.WriteLine($"Retry {retryAttempt} after {timeSpan.Seconds} seconds due to: {ex.Message}");
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
        }

        ///// <summary>
        ///// Provides access too all generated output files only. These are files that were output as a result of file processing some input files.
        ///// </summary>
        public IDirectory GeneratedOutput { get; set; }

        /// <summary>
        /// Provides access too all un-processed output files only. These are basically input files that need to be seen by the brwoser, but on which
        /// no processing occurred. For example, a processor that compiles typescript input files, and produces js output files may need to also 
        /// ensure the orgiginal typescript files can be served up to the browser when source maps are enabled. The typescript files are the original
        /// input files and no actual changes are being made to those files so they are placed in the UnprocessedOutputFileProvider which
        /// get's integrated with the Environments WebRoot file provider so that the files can be resolved.
        /// </summary>
        public IDirectory SourcesOutput { get; set; }

        /// <summary>
        /// Provides access to environment files and generated files.
        /// </summary>
        public IFileProvider FileProvider { get; set; }

        //public PipeProcessor PipeContext { get; set; }

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
            SourcesOutput.AddOrUpdateFile(directory, fileInfo);
        }

        /// <summary>
        /// Adds the generated file as an output of the pipeline.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="file"></param>
        public void AddGeneratedOutput(string directory, IFileInfo file)
        {
            GeneratedOutput.AddOrUpdateFile(directory, file);
        }

    }
}