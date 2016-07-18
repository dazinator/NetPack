using System.IO;
using Microsoft.Extensions.FileProviders;

namespace NetPack.Tests
{
    public static class TestUtils
    {
        /// <summary>
        /// Returns a mock file provider that returns `IFileInfo` objects for the specified paths when they are requested.
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        public static IFileProvider GetMockFileProvider(string[] filePaths)
        {
            return GetMockFileProvider(filePaths, null);

        }

        /// <summary>
        /// Returns a mock file provider that returns `IFileInfo` objects for the specified paths when they are requested. If there is
        /// a content element at the same index as the requested filePath in the contents array, then that content will be returned if the IFileInfo
        /// is subsequently Read.
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static IFileProvider GetMockFileProvider(string[] filePaths, string[] contents)
        {
            var mockFileProvider = new Moq.Mock<IFileProvider>();

            int index = 0;
            foreach (var filePath in filePaths)
            {
                var mockSomeFile = new Moq.Mock<IFileInfo>();
                mockSomeFile.Setup(a => a.Exists).Returns(true);
                mockSomeFile.Setup(a => a.IsDirectory).Returns(false);

                var fileName = Path.GetFileName(filePath);
                mockSomeFile.Setup(a => a.Name).Returns(fileName);
                mockFileProvider.Setup(a => a.GetFileInfo(filePath)).Returns(mockSomeFile.Object);

                //MemoryStream stream = null;
                if (contents != null &&
                    contents.Length >= index + 1)
                {
                    var content = contents[index];

                    mockSomeFile.Setup(a => a.CreateReadStream()).Returns(() =>
                    {
                        var stream = new MemoryStream();
                        StreamWriter writer = new StreamWriter(stream);
                        writer.Write(content);
                        writer.Flush();
                        stream.Position = 0;
                        return stream;
                    });

                }
            }

            return mockFileProvider.Object;

        }

    }
}