using NetPack.Pipeline;
using Xunit;
using System.Linq;

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

            Assert.True(sut.Input.GetIncludes().Count() == 2);

        }

       
    }
}