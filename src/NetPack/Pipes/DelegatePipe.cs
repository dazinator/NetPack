using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Pipeline
{

    public class DelegatePipe : BasePipe
    {

        private readonly Func<PipeState, CancellationToken, Task> _processAsync;

        public DelegatePipe(Func<PipeState, CancellationToken, Task> processAsync, string name = "Delegate"):base(name)
        {
            _processAsync = processAsync;
        }
        public override async Task ProcessAsync(PipeState context, CancellationToken cancellationToken)
        {
            await _processAsync(context, cancellationToken);
        }
    }
}