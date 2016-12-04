using Dazinator.AspNet.Extensions.FileProviders;
using NetPack.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var input = new PipelineInput();
            input.AddInclude("wwwroot/*.js");
            var results = input.GetFiles(fileProvider);

            Assert.Equal(1, results.Length);
            Assert.Equal("wwwroot/somefile.js", results[0].FileSubPath);

        }


    }
}
