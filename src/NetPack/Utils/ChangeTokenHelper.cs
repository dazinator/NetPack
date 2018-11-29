using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.Utils
{
    public static class ChangeTokenHelper
    {
        private static readonly ConcurrentDictionary<object, CancellationTokenSource> Tokens
            = new ConcurrentDictionary<object, CancellationTokenSource>();

        private const int DefaultDelay = 1000;      

        /// <summary>
        /// Handle <see cref="ChangeToken.OnChange{TState}(Func{IChangeToken}, Action{TState}, TState)"/> after a delay that discards any duplicate invocations within that period of time.
        /// Useful for working around issue like described here: https://github.com/aspnet/AspNetCore/issues/2542
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="changeTokenFactory"></param>
        /// <param name="listener"></param>
        /// <param name="state"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static IDisposable OnChangeDelayed<T>(Func<IChangeToken> changeTokenFactory, Action<T> listener, T state, int delay = DefaultDelay)
        {
            var token = ChangeToken.OnChange<T>(changeTokenFactory, (s) => ChangeHandler(listener, s), state);
            return token;          
        }

        private static void ChangeHandler<T>(Action<T> listener, T obj)
        {
            var tokenSource = GetCancellationTokenSource(obj);
            var token = tokenSource.Token;
            var delay = Task.Delay(DefaultDelay, token);

            delay.ContinueWith(
                _ => ListenerInvoker(obj, listener, obj),
                token
                );
        }

        private static CancellationTokenSource GetCancellationTokenSource<T>(T key)
        {
            return Tokens.AddOrUpdate(key, CreateTokenSource, ReplaceTokenSource);
        }

        private static CancellationTokenSource CreateTokenSource(object key)
        {
            return new CancellationTokenSource();
        }

        private static CancellationTokenSource ReplaceTokenSource(object key, CancellationTokenSource existing)
        {
            existing.Cancel();
            existing.Dispose();
            return new CancellationTokenSource();
        }

        private static void ListenerInvoker<T>(T key, Action<T> listener, T obj)
        {
            listener(obj);
            if (Tokens.TryRemove(key, out var tokenSource))
            {
                tokenSource.Dispose();
            }
        }
    }
}
