using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.FileInfo;
using Microsoft.Extensions.FileProviders;
using NetPack.Tests.Pipes;
using System;
using System.IO;
using System.IO.Compression;

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

            var subPath = SubPathInfo.Parse(path);
            var fileInfo = new MemoryStreamFileInfo(stream, null, subPath.Name);
            testBase.Directory.AddFile(subPath.Directory, fileInfo);

            return new FileWithDirectory() { Directory = subPath.Directory, FileInfo = fileInfo };

            //return fileInfo;
        }
    }
}
