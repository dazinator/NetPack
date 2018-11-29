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

        protected BasePipe(string name)
        {
            Name = name;
        }

        public abstract Task ProcessAsync(PipeState context, CancellationToken cancelationToken);

        public string Name { get; set; }

    }
}