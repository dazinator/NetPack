using Dazinator.Extensions.FileProviders;
using NetPack.Tests.Pipes;
using System;
using System.IO;
using System.IO.Compression;
using NetPack.Utils;

namespace NetPack.Zip.Tests
{
    public static class PipeTestBaseExtensions
    {

        public static FileWithDirectory GivenAZipArchiveFileInfo(this PipeTestBase testBase, string path, Action<ZipArchiveBuilder> builder)
        {
            var stream = new MemoryStream();
            
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Update, true))
            {
                var archiveBuilder = new ZipArchiveBuilder(archive);
                builder(archiveBuilder);   
            }
            stream.Position = 0;

         //   throw new NotImplementedException("SubPathInfo gone");
            var subPath = SubPathInfo.Parse(path);
            var fileInfo = new MemoryStreamFileInfo(stream, subPath.Name);
            testBase.Directory.AddFile(subPath.Directory, fileInfo);
            return new FileWithDirectory() { Directory = subPath.Directory, FileInfo = fileInfo };
           
        }
    }
}
