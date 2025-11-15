using Microsoft.AspNetCore.Http;
using Xunit;
using Xunit.Abstractions;

namespace NetPack.Tests
{
    public class PathStringHelperTests
    {
        private ITestOutputHelper _output;


        public PathStringHelperTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("/foo/bar.txt", "/foo", "bar.txt")]
        [InlineData("foo/bar.txt", "/foo", "bar.txt")]
        [InlineData("foo.txt", "/", "foo.txt")]
        [InlineData("foo", "/", "foo")]
        [InlineData("foo/", "/foo", "")]
        [InlineData("foo/bar", "/foo", "bar")]
        [InlineData("foo/.hidden", "/foo/.hidden", "")]
        [InlineData("foo/ba?.txt", "/foo/ba?.txt", "")]
        [InlineData("foo/*.txt", "/foo/*.txt", "")]
        public void SplitPath_ShouldReturn_CorrectDirectoryAndFileName(string input, string expectedDir, string expectedFile)
        {
            // Arrange
            PathString pathString = input.ToPathString();

            // Act
            var result = PathStringHelper.SplitPath(pathString);

            // Assert
            Assert.Equal(new PathString(expectedDir), result.Directory);
            Assert.Equal(expectedFile, result.FileName);
        }


    }
}