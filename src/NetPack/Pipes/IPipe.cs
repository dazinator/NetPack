using System.Threading;
using System.Threading.Tasks;
using NetPack.Pipeline;

namespace NetPack
{
    public interface IPipe
    {
        Task ProcessAsync(PipeContext context, CancellationToken cancelationToken);
    }

    //public interface IPipeOutput
    //{
    //    /// <summary>
    //    /// The files that should be passed on to the next stage of the pipeline, as output from this pipe.
    //    /// </summary>
       
    //}
}