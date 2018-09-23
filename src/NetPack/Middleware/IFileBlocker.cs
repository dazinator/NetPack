using System;

namespace NetPack
{
    public interface IFileBlocker : IDisposable
    {
        IFileBlocker AddBlock(string path);
        IFileBlocker AddBlock(FileWithDirectory file);
      //  IFileBlocker AddBlocks(string[] paths);
       // IFileBlocker AddBlocks(FileWithDirectory[] files);
    }
}