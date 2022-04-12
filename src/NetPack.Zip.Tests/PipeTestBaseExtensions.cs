using Dazinator.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
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

            PathStringUtils.GetPathAndFilename(path, out var directory, out var name);

            //  var subPath = SubPathInfo.Parse(path);
            var fileInfo = new MemoryStreamFileInfo(stream, name);
            testBase.Directory.AddFile(directory, fileInfo);

            return new FileWithDirectory() { Directory = directory, FileInfo = fileInfo };

            //return fileInfo;
        }
    }
}
