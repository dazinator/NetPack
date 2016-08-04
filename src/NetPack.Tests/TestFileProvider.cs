using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace NetPack.Tests
{
    public class TestFileProvider : IFileProvider
    {

        private readonly IFileProvider _fileProvider;
        public TestFileProvider(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            Files = new Dictionary<string, IFileInfo>();
        }

        public Dictionary<string, IFileInfo> Files { get; set; }

        public IFileInfo GetFileInfo(string subpath)
        {
            return Files.ContainsKey(subpath) ? Files[subpath] : _fileProvider.GetFileInfo(subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _fileProvider.GetDirectoryContents(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _fileProvider.Watch(filter);
        }
    }
}