using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using static NetPack.FileLocking.CompositeFileLock;

namespace NetPack.FileLocking
{

    public class CompositeFileLock
    {

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private CountdownEvent _cte = new CountdownEvent(1);
        private readonly Action _onExpired;


        private readonly int _counter = 1;

        public CompositeFileLock(Action onExpired)
        {
            _onExpired = onExpired;
        }

        public void IncrementLockCount()
        {
            _cte.AddCount();
            // return new FileLock(this);
        }

        public void RemoveLock()
        {
            try
            {
                _cte.Signal();
                if (_cte.IsSet)
                {
                    _onExpired();
                }
            }
            catch (Exception e)
            {

                throw;
            }

        }

        public Task Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (_cte.IsSet)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                _cte.Wait(timeout, cancellationToken);
            }, cancellationToken);
        }

        internal class FileLock : IDisposable
        {
            private CompositeFileLock _cfl;

            public FileLock(CompositeFileLock cfl)
            {
                _cfl = cfl;
            }

            public void Dispose()
            {
                _cfl.RemoveLock();
            }
        }

    }

    public static class FileLocks
    {

        private static readonly ConcurrentDictionary<PathString, CompositeFileLock> _locks = new ConcurrentDictionary<PathString, CompositeFileLock>();

        public static IDisposable RegisterLock(PathString path)
        {
            CompositeFileLock locker = _locks.AddOrUpdate(path, (p) =>
             {
                 return new CompositeFileLock(() =>
                 {
                     _locks.TryRemove(path, out CompositeFileLock l);
                 });
             }, (key, existing) =>
             {
                 existing.IncrementLockCount();
                 return existing;
             });

            return new FileLock(locker);
        }

        public static async Task WaitIfLockedAsync(PathString path, TimeSpan timeout, CancellationToken ct)
        {
            if (_locks.TryGetValue(path, out CompositeFileLock locker))
            {
                await locker.Wait(timeout, ct);
            }
            return;
        }

    }

    public class FileLocker : IDisposable, IFileBlocker
    {

        // private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly CompositeDisposable _locks = new CompositeDisposable();

        public FileLocker()
        {

        }

        public void AddLock(PathString path)
        {
            _locks.Add(FileLocks.RegisterLock(path));
        }

        public IFileBlocker AddBlock(string path)
        {
            _locks.Add(FileLocks.RegisterLock(path));
            return this;
        }

        public IFileBlocker AddBlock(FileWithDirectory file)
        {
            _locks.Add(FileLocks.RegisterLock(file.UrlPath));
            return this;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _locks.Dispose();
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FileBlocker() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
