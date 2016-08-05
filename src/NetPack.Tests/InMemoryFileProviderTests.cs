using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using NetPack.File;
using NetPack.Pipeline;
using Xunit;

namespace NetPack.Tests
{
    public class InMemoryFileProviderTests
    {
        [Fact]
        public void Can_Add_Files_And_Then_Get_Them()
        {

            var fileProvider = new InMemoryFileProvider();
            var content = "hi there";

            // act
            fileProvider.AddFile("/myfolder/somefile.txt", content);
            // get the file back, testing two paths with and wothout a slash prefix.
            var file = fileProvider.EnsureFile("myfolder/somefile.txt");
            var sameFile = fileProvider.EnsureFile("/myfolder/somefile.txt");

            Assert.Equal(file.ReadAllContent(), content);
            Assert.Same(file, sameFile);

            Assert.True(file.Exists);
            Assert.False(file.IsDirectory);

            Assert.Equal(file.Name, "somefile.txt");
            Assert.Null(file.PhysicalPath);
            Assert.True(file.Length > 0);

        }

        [Fact]
        public void Watching_The_Same_File_Gets_Same_Change_Token()
        {

            var fileProvider = new InMemoryFileProvider();
            var content1 = "hi there";
            fileProvider.AddFile("/myfolder/somefile.txt", content1);

            // act
            var token = fileProvider.Watch("/myfolder/somefile.txt");
            var token2 = fileProvider.Watch("myfolder/somefile.txt");

            Assert.Same(token, token2);

        }


        [Fact]
        public void Can_Watch_A_Single_File_For_Changes()
        {

            var fileProvider = new InMemoryFileProvider();
            var content1 = "hi there";

            // act
            var path = SubPathInfo.Parse("/myfolder/somefile.txt");
            fileProvider.AddFile(path, content1);

            // watch all txt files.
            var token = fileProvider.Watch(path.ToString()) as InMemoryChangeToken;
            bool changeFired = false;

            token.RegisterChangeCallback((a) =>
            {
                changeFired = true;
            }, null);

            // Update the file. Should trigger the change callback.
            fileProvider.UpdateFile(path, new StringFileInfo("new contents", "somefile.txt"));
            
            Assert.True(changeFired);

        }

        [Fact]
        public void Can_Watch_Multiple_Files_For_Changes()
        {

            var fileProvider = new InMemoryFileProvider();
            var content1 = "hi there";
            var content2 = "more content";

            // act
            var path1 = SubPathInfo.Parse("/myfolder/somefile.txt");
            var path2 = SubPathInfo.Parse("/myfolder/somefile2.txt");
            var path3 = SubPathInfo.Parse("/myfolder/notinterested.txt");
            fileProvider.AddFile(path1, content1);
            fileProvider.AddFile(path2, content2);
            fileProvider.AddFile(path3, "not interested in this one");

            // watch all txt files.
            var token = fileProvider.Watch("/myfolder/some*.txt") as IChangeToken;
            int changeFiredCount = 0;

            token.RegisterChangeCallback((a) =>
            {
                changeFiredCount = changeFiredCount + 1;
            }, null);

            // Update the file. Should trigger the change callback.
            fileProvider.UpdateFile(path1, new StringFileInfo("changed 1", path1.FileName));
            Assert.Equal(changeFiredCount, 1);

            fileProvider.UpdateFile(path2, new StringFileInfo("changed 2", path2.FileName));
            Assert.Equal(changeFiredCount, 2);

            fileProvider.UpdateFile(path3, new StringFileInfo("changed 3", path3.FileName));
            Assert.Equal(changeFiredCount, 2);
        }

    }
}