using Xunit;
using Xunit.Abstractions;

namespace NetPack.Tests
{
    public class SubpathHelperTests
    {
        private ITestOutputHelper _output;


        public SubpathHelperTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("/some/output", "/jsfiles/jsfile.js", "../../jsfiles/jsfile.js")]
        [InlineData("/some/output/", "/jsfiles/jsfile.js", "../../jsfiles/jsfile.js")]
        [InlineData("/", "jsfiles/jsfile.js", "jsfiles/jsfile.js")]
        [InlineData("", "/ts/another.js.map", "ts/another.js.map")]        
        public async void Can_Make_Relative_Subpath(string fromPath, string toPath, string expected)
        {
            // given a directory path, and also some file path, where they both share a common root directory,
            // can get the relative path from that directory to that file.

            var result = SubpathHelper.MakeRelativeSubpath(fromPath, toPath);
            Assert.Equal(expected, result);


        }


    }
}