using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Pipeline
{

    public class DelegatePipe : IPipe
    {

        private Func<IPipelineContext, CancellationToken, Task> _processAsync;

        public DelegatePipe(Func<IPipelineContext, CancellationToken, Task> processAsync)
        {
            _processAsync = processAsync;
        }
        public async Task ProcessAsync(IPipelineContext context, CancellationToken cancellationToken)
        {
            await _processAsync(context, cancellationToken);
        }
    }
}