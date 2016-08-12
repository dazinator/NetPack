using System.IO;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using NetPack.Pipeline;
using Xunit;

namespace NetPack.Tests
{
    public class SourcesBuilderTests
    {
        [Fact]
        public void Can_Specify_Source_Files()
        {

            var fileProvider = new InMemoryFileProvider();
            fileProvider.AddFile("wwwroot/somefile.ts", string.Empty);
            fileProvider.AddFile("wwwroot/someOtherfile.ts", string.Empty);
          //  TestUtils.GetMockFileProvider(new[] { "wwwroot/somefile.ts", "wwwroot/someOtherfile.ts" });

            var sut = new PipelineInputBuilder(fileProvider);
            sut
                .Include("wwwroot/somefile.ts")
                .Include("wwwroot/someOtherfile.ts");

            Assert.True(sut.Input.Files.Count == 2);

        }

       
    }
}