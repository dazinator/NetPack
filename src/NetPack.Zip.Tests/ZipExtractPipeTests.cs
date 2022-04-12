using NetPack.Tests.Pipes;
using System.Threading.Tasks;
using Xunit;


namespace NetPack.Zip.Tests
{
    public class ZipExtractPipeTests : PipeTestBase
    {

        [Fact]
        public async Task Extracts_Zip_Files()
        {

            var fooZip = this.GivenAZipArchiveFileInfo("foo.zip", (builder) =>
            {
                builder.AddFile("foo.txt", System.Text.Encoding.Default.GetBytes("foo bar"));
            });

            //  var zipFiles = new FileWithDirectory[] { new FileWithDirectory() { Directory = "/test", FileInfo = fooZip } };

            await WhenFilesProcessedByPipe(() =>
            {
                var options = new ZipExtractPipeOptions();
                return new ZipExtractPipe(options);
            }, fooZip);


            // The the extracted file should be output.
            ThenTheProcessedOutputDirectoryFile("foo.txt", Assert.NotNull);


            // Content of file should match
            ThenTheProcessedOutputDirectoryFile("foo.txt", (unzippedFile) =>
            {
                var content = unzippedFile.ReadAllContent();
                Assert.Equal("foo bar", content);
            });

        }

    }

}
