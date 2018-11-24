using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Pipeline
{
    public abstract class BasePipe : IPipe
    {
        //protected virtual IFileBlocker UseFileBlocker()
        //{
        //    return new FileBlocker();
        //}

        public abstract Task ProcessAsync(PipeState context, CancellationToken cancelationToken);

    }
}