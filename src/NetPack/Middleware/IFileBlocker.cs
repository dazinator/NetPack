using System;

namespace NetPack
{
    public interface IFileBlocker : IDisposable
    {
        IFileBlocker AddBlock(string path);
    }
}