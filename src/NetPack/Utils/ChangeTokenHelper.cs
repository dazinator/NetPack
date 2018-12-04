using Microsoft.Extensions.Primitives;
using System;

namespace NetPack.Utils
{
    public static class ChangeTokenHelper
    {     
        private const int DefaultDelayInMilliseconds = 500;

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
        public static IDisposable OnChangeDebounce<T>(Func<IChangeToken> changeTokenFactory, Action<T> listener, T state, int delayInMilliseconds = DefaultDelayInMilliseconds)
        {
            var debouncer = new Debouncer<T>(TimeSpan.FromMilliseconds(delayInMilliseconds));
            var token = ChangeToken.OnChange<T>(changeTokenFactory, s => debouncer.Debounce(listener, s), state);
            return token;
        }       

    }
}
