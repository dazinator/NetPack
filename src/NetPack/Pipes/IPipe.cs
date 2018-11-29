using NetPack.Pipeline;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack
{
    public interface IPipe
    {
        Task ProcessAsync(PipeState context, CancellationToken cancelationToken);
        string Name { get; set; }
    }

    //public interface IPipeOutput
    //{
    //    /// <summary>
    //    /// The files that should be passed on to the next stage of the pipeline, as output from this pipe.
    //    /// </summary>

    //}
}