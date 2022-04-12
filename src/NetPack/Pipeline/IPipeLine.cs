using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Pipeline
{
    public interface IPipeLine
    {

        List<PipeProcessor> Pipes { get; }

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
        Task ProcessPipesAsync(IEnumerable<PipeProcessor> pipes, CancellationToken none);

        /// <summary>
        /// Processes only the specified pipe.
        /// </summary>       
        /// <returns></returns>
        Task ProcessPipe(PipeProcessor pipe, CancellationToken cancellationToken);

        /// <summary>
        /// Processes only the specified pipes.
        /// </summary>
        /// <param name="pipes"></param>
        /// <param name="none"></param>
        /// <returns></returns>
        Task ProcessUninitialisedPipesAsync(CancellationToken none);

        IFileProvider EnvironmentFileProvider { get; set; }

        IFileProvider GeneratedOutputFileProvider { get; set; }

        IFileProvider SourcesFileProvider { get; set; }

        IFileProvider WebrootFileProvider { get; set; }

        void Initialise();

        bool IsBusy
        {
            get;
        }
        //bool HasUninitialisedPipes();

    }
}