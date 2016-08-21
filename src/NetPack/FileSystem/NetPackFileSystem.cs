using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using NetPack.FileSystem;

namespace NetPack.File
{
    public class NetPackFileSystem
    {
        public NetPackFileSystem() : this(new NetPackFileDirectory(null, null))
        {

        }

        protected NetPackFileSystem(NetPackFileDirectory rootDir)
        {
            Directory = rootDir;
        }

        public NetPackFileDirectory Directory { get; set; }

        public void AddFile(string directory, IFileInfo file)
        {
            // for each segment in the directory, get nested directory.
            var dir = Directory.EnsureDirectory(directory);
            dir.AddFile(file);
        }

        //public IFileInfo[] GetFilesInFolder(string path)
        //{
        //    var dir = Directory.EnsureDirectory(path);
        //    var files = new List<IFileInfo>();
        //    foreach (var key in dir.Files)
        //    {
        //        var file = key.Value;
        //        files.Add(file);
        //    }
        //    return files.ToArray();
        //}
    }
}