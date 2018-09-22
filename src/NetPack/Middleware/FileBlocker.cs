using System;
using System.Collections.Generic;

namespace NetPack
{
    public class FileBlocker : IFileBlocker
    {

        private readonly List<IDisposable> _locks = new List<IDisposable>();


        public FileBlocker()
        {

        }

        public IFileBlocker AddBlock(string path)
        {
            _locks.Add(FileRequestServices.BlockFilePath(path));
            return this;
        }

        public IFileBlocker AddBlocks(string[] paths)
        {
            foreach (var path in paths)
            {
                _locks.Add(FileRequestServices.BlockFilePath(path));
            }           
            return this;
        }

        public IFileBlocker AddBlocks(FileWithDirectory[] files)
        {
            foreach (var file in files)
            {
                _locks.Add(FileRequestServices.BlockFilePath(file.FileSubPath));
            }
            return this;
        }

        public IFileBlocker AddBlock(FileWithDirectory file)
        {
            _locks.Add(FileRequestServices.BlockFilePath(file.FileSubPath));
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
                    // TODO: dispose managed state (managed objects).
                    using (new CompositeDisposable(_locks))
                    {
                    }
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