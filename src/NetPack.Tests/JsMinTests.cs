using NetPack.File;
using Xunit;
using Xunit.Abstractions;

namespace NetPack.Tests
{
    public class JsMinTests
    {

        public const string JsContentOne = "define(\"IContentOne\", [\"require\", \"exports\"], function (require, exports) { \"use strict\"; });";
        //public const string JsContentTwo = "define(\"IContentTwo\", [\"require\", \"exports\"], function (require, exports) { \"use strict\"; });";

        private ITestOutputHelper _output;
        public JsMinTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async void Can_Minify_Source_Files()
        {
            var fileInfo = new StringFileInfo("somefile.js", JsContentOne);
            var sut = new JsMinifier();
            var context = new FileProcessContext(new SourceFile(fileInfo, "wwwroot"));

            var result = await sut.ProcessInputAsync(context);

            _output.WriteLine(result);
            Assert.True(result.Length < JsContentOne.Length);
            _output.WriteLine($"Minification reduced length by: {JsContentOne.Length - result.Length} characters.");

        }



    }
}