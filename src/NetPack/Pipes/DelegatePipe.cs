using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Pipeline
{

    public class DelegatePipe : BasePipe
    {

        private readonly Func<PipeContext, CancellationToken, Task> _processAsync;

        public DelegatePipe(Func<PipeContext, CancellationToken, Task> processAsync)
        {
            _processAsync = processAsync;
        }
        public override async Task ProcessAsync(PipeContext context, CancellationToken cancellationToken)
        {
            await _processAsync(context, cancellationToken);
        }
    }
}