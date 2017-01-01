using NetPack.Tests.Pipes;
using System.Threading.Tasks;
using Xunit;

namespace NetPack.JsMin.Tests
{
    public class JsMinifyPipeTests : PipeTestBase
    {

        [Fact]
        public async Task Minifies_Js_File()
        {


            // Given
            var jsFile = GivenAFileInfo("SomeFolder/somefile.js", () => JsContentOne);

            // When file processed by the typescript compile pipe
            await WhenFilesProcessedByPipe(() =>
            {
                var options = new JsMinOptions();
                return new JsMinifierPipe(options);
            }, jsFile);


            // Then
            ThenTheProcessedOutputDirectoryFile("SomeFolder/somefile.min.js", Assert.NotNull);
            ThenTheProcessedOutputDirectoryFile("SomeFolder/somefile.min.js.map", Assert.NotNull);

            ThenTheProcessedOutputDirectoryFile("SomeFolder/somefile.min.js.map", (file) =>
            {
                var contents = file.ReadAllContent();

            }
            );

        }


        public const string JsContentOne = @"function process(count)
{
     var value = """";
}";


    }
}
