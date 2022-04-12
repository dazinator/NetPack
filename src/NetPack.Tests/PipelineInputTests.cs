using Dazinator.Extensions.FileProviders;
using Dazinator.Extensions.FileProviders.InMemory;
using NetPack.Pipeline;
using Xunit;

namespace NetPack.Tests
{
    public class PipelineInputTests
    {

        [Fact]
        public void Can_Match_All_Js_In_Specific_Directory()
        {

            var fileProvider = new InMemoryFileProvider();
            fileProvider.Directory.AddFile("wwwroot", new StringFileInfo("hi", "somefile.js"));
            fileProvider.Directory.AddFile("wwwroot", new StringFileInfo("there", "someOtherfile.js.map"));

            var input = new PipeInput();
            input.AddInclude("wwwroot/*.js");
            var results = fileProvider.GetFiles(input);

            Assert.Equal(1, results.Length);
            Assert.Equal("/wwwroot/somefile.js", results[0].UrlPath);

        }


    }
}
