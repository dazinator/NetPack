using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Utils
{      
    /// <summary>
    /// Courtesy of @cocowalla https://gist.github.com/cocowalla/5d181b82b9a986c6761585000901d1b8
    /// </summary>
    public class Debouncer : IDisposable
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly TimeSpan waitTime;
        private int counter;

        public Debouncer(TimeSpan? waitTime = null)
        {
            this.waitTime = waitTime ?? TimeSpan.FromSeconds(1);
        }

        public void Debouce(Action action)
        {
            int current = Interlocked.Increment(ref counter);

            Task.Delay(waitTime).ContinueWith(task =>
            {
                // Is this the last task that was queued?
                if (current == counter && !cts.IsCancellationRequested)
                {
                    action();
                }
                else
                {
                    Console.WriteLine("debounced..");
                }
#if NETSTANDARD2_0
                  task.Dispose();
#endif

            }, cts.Token);
        }

        public void Dispose()
        {
            cts.Cancel();
        }
    }

    public class Debouncer<T> : IDisposable
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly TimeSpan waitTime;
        private int counter;

        public Debouncer(TimeSpan? waitTime = null)
        {
            this.waitTime = waitTime ?? TimeSpan.FromSeconds(1);
        }

        public void Debounce(Action<T> action, T state)
        {
            int current = Interlocked.Increment(ref counter);

            Task.Delay(waitTime).ContinueWith(task =>
            {
                // Is this the last task that was queued?
                if (current == counter && !cts.IsCancellationRequested)
                {
                    action(state);
                }
                else
                {
                    Console.WriteLine("debounced..");
                }
#if NETSTANDARD2_0
                  task.Dispose();
#endif

            }, cts.Token);
        }

        public void Dispose()
        {
            cts.Cancel();
        }
    }
}
