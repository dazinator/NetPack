using System.IO;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using NetPack.Pipeline;
using Xunit;
using Dazinator.AspNet.Extensions.FileProviders;

namespace NetPack.Tests
{
    public class PipelineInputBuilderTests
    {
        [Fact]
        public void Can_Specify_Source_Files()
        {

            var sut = new PipelineInputBuilder();
            sut
                .Include("wwwroot/somefile.ts")
                .Include("wwwroot/someOtherfile.ts");

            Assert.True(sut.Input.IncludeList.Count == 2);

        }

       
    }
}