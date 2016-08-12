using System;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace NetPack.File
{
    public class NetPackDirectoryInfo : IFileInfo
    {
        public NetPackDirectoryInfo(string name)
        {
            Name = name;
            IsDirectory = true;
        }
        public Stream CreateReadStream()
        {
            throw new InvalidOperationException("Cannot create a stream for a directory.");
        }

        public bool Exists { get; } = true;
        public long Length { get; } = -1;
        public string PhysicalPath { get; } = null;
        public string Name { get; }
        public DateTimeOffset LastModified { get; }
        public bool IsDirectory { get; }
    }
}