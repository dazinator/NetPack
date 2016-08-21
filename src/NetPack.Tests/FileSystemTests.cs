using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using NetPack.FileSystem;
using NetPack.Pipes;
using NetPack.Pipes.Combine;
using Xunit;
using Xunit.Abstractions;

namespace NetPack.Tests
{
    public class FileSystemTests
    {
        private ITestOutputHelper _output;

        public FileSystemTests(ITestOutputHelper output)
        {
            _output = output;
            Sut = new NetPackFileSystem();
        }

        [Fact]
        public async void Can_Add_Files_To_Directory_Then_Get_Them()
        {
            // set up a couple of in memory files, for fileA and fileB
            // that both have source mapping url comments.

            var fileA = GivenFileSystemHasFile("wwwroot/fileA.js", 8000);
            var fileB = GivenFileSystemHasFile("wwwroot/fileB.js", 1000);

            WhenDirectoryIs("wwwroot");

            ThenDirectoryHasFiles("fileA.js", "fileB.js");


        }

        private void ThenDirectoryHasFiles(params string[] fileName)
        {
            foreach (var file in fileName)
            {
                Assert.NotNull(CurrentDirectory.Files.ContainsKey(file));
            }
        }

        private void WhenDirectoryIs(string path)
        {
            CurrentDirectory = Sut.Directory.EnsureDirectory(path);

        }

        public NetPackFileDirectory CurrentDirectory { get; set; }

        public NetPackFileSystem Sut { get; set; }

        public IFileInfo FilesInFolder { get; set; }

        private IFileInfo GivenFileSystemHasFile(string path, int length)
        {
            var subPath = SubPathInfo.Parse(path);
            var content = TestUtils.GenerateString(length);
            var fileInfo = new StringFileInfo(content, subPath.Name);
            Sut.AddFile(subPath.Directory, fileInfo);
            return fileInfo;
        }


    }
}