using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Pipeline
{

    public class DelegatePipe : IPipe
    {

        private Func<PipeContext, CancellationToken, Task> _processAsync;

        public DelegatePipe(Func<PipeContext, CancellationToken, Task> processAsync)
        {
            _processAsync = processAsync;
        }
        public async Task ProcessAsync(PipeContext context, CancellationToken cancellationToken)
        {
            await _processAsync(context, cancellationToken);
        }
    }
}