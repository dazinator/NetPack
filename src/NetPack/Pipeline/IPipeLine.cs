using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.Extensions.FileProviders;

namespace NetPack.Pipeline
{
    public interface IPipeLine
    {
        IDirectory GeneratedOutputDirectory { get; set; }

        List<PipeContext> Pipes { get; }

        PipelineContext Context { get; set; }

        /// <summary>
        /// Processes all pipes in the pipeline.
        /// </summary>
        /// <param name="cancelationToken"></param>
        /// <returns></returns>
        Task ProcessAsync(CancellationToken cancelationToken);

        /// <summary>
        /// Processes only the specified pipes.
        /// </summary>
        /// <param name="pipes"></param>
        /// <param name="none"></param>
        /// <returns></returns>
        Task ProcessPipesAsync(IEnumerable<PipeContext> pipes, CancellationToken none);

        /// <summary>
        /// Processes only the specified pipes.
        /// </summary>
        /// <param name="pipes"></param>
        /// <param name="none"></param>
        /// <returns></returns>
        Task ProcessDirtyPipesAsync(CancellationToken none);

        IFileProvider EnvironmentFileProvider { get; set; }

        IFileProvider GeneratedOutputFileProvider { get; set; }

        IFileProvider InputAndGeneratedFileProvider { get; set; }

        IDirectory SourcesOutputDirectory { get; set; }

        IFileProvider SourcesFileProvider { get; set; }

        IFileProvider WebrootFileProvider { get; set; }
               
        string BaseRequestPath { get; set; }
      
        void Initialise();

        bool HasDirtyPipes();
      
    }
}