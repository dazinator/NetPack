using System.IO;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace NetPack.Tests
{
    public class SourcesBuilderTests
    {
        [Fact]
        public void Can_Specify_Source_Files()
        {

            var fileProvider = TestUtils.GetMockFileProvider(new[] { "wwwroot/somefile.ts", "wwwroot/someOtherfile.ts" });

            var sut = new SourcesBuilder(fileProvider);
            sut
                .Include("wwwroot/somefile.ts")
                .Include("wwwroot/someOtherfile.ts");

            Assert.True(sut.SourceFiles.Count == 2);

        }

       
    }
}