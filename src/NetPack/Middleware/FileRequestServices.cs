using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack
{
    public static class FileRequestServices
    {
        private static ConcurrentDictionary<string, ConcurrentDictionary<Guid, FileRequestLock>> _busyFiles = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, FileRequestLock>>();

        /// <summary>
        /// Returns a task that can be awaited so that control resumes only when a file is no longer locked by a file request lock.
        /// </summary>
        /// <param name="subPath"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task WhenFileNotLocked(string subPath, TimeSpan timeout, CancellationToken cancellationToken)
        {
            ConcurrentDictionary<Guid, FileRequestLock> existingLocks;
            _busyFiles.TryGetValue(subPath, out existingLocks);
            if (existingLocks == null || existingLocks.Count == 0)
            {
                return Task.FromResult<bool>(true);
            };

            return Task.Factory.StartNew(() =>
            // return new Task(() =>
            {

                foreach (var item in existingLocks)
                {
                    item.Value.Event.Wait(timeout, cancellationToken);
                    // once its been signalled, remove the lock.                    
                }

                // WaitHandle.wa(events, timeout, cancellationToken);
                //existingLock.Event.Wait(timeout, cancellationToken);
            });

        }

        /// <summary>
        /// Creates a lock that blocks a file from being served until the lock is disposed. Enforced by middleware.
        /// </summary>
        /// <param name="subPath"></param>
        /// <returns></returns>
        public static IDisposable BlockFilePaths(params string[] subPaths)
        {
            if (subPaths == null)
            {
                throw new ArgumentNullException(nameof(subPaths));
            }

            List<IDisposable> disposables = new List<IDisposable>(subPaths.Length);
            foreach (var item in subPaths)
            {
                disposables.Add(BlockFilePath(item));
            }
            var compositeDisposable = new CompositeDisposable(disposables);
            return compositeDisposable;
        }

        /// <summary>
        /// Creates a lock that blocks a file from being served until the lock is disposed. Enforced by middleware.
        /// </summary>
        /// <param name="subPath"></param>
        /// <returns></returns>
        public static IDisposable BlockFilePath(string subPath)
        {
            if (string.IsNullOrWhiteSpace(subPath))
            {
                throw new ArgumentNullException(nameof(subPath));
            }

            if (!subPath.StartsWith("/"))
            {
                subPath = "/" + subPath;
            }

            var objectLock = new FileRequestLock(subPath, (o) =>
            { // remove the lock from the bag when it is disposed.
                ConcurrentDictionary<Guid, FileRequestLock> currentLocks;
                if (_busyFiles.TryGetValue(subPath, out currentLocks))
                {
                    FileRequestLock removed;
                    currentLocks.TryRemove(o.LockId, out removed);
                };
            });
            var locks = _busyFiles.GetOrAdd(subPath, (a) =>
             {
                 var newDictionary = new ConcurrentDictionary<Guid, FileRequestLock>();
                 return newDictionary;
             });

            locks.TryAdd(objectLock.LockId, objectLock);
            return objectLock;
        }

        internal class FileRequestLock : IDisposable
        {
            private readonly string _subpath;
            private readonly CountdownEvent _event;
            private readonly Action<FileRequestLock> _onDisposing;
            private readonly Guid _lockId;

            internal CountdownEvent Event { get { return _event; } }

            internal string Subpath { get { return _subpath; } }

            internal Guid LockId { get { return _lockId; } }

            public FileRequestLock(string subPath, Action<FileRequestLock> onDisposing)
            {
                _lockId = Guid.NewGuid();
                _subpath = subPath;
                _event = new CountdownEvent(1);
                _onDisposing = onDisposing;
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).
                        // signal
                        _event.Signal();
                        _event.Dispose();
                        _onDisposing(this);
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            // ~FileRequestLock() {
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

        public static bool HasLocks()
        {
            foreach (var item in _busyFiles.Values)
            {
                if (!item.IsEmpty)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasLock(string subPath)
        {
            ConcurrentDictionary<Guid, FileRequestLock> existingLocks;
            _busyFiles.TryGetValue(subPath, out existingLocks);
            return !(existingLocks == null || existingLocks.Count == 0);
        }

    }
}