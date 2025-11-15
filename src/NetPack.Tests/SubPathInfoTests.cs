using Xunit.Abstractions;

namespace NetPack.Tests
{
    public class SubPathInfoTests
    {
        private ITestOutputHelper _output;
     

        public SubPathInfoTests(ITestOutputHelper output)
        {
            _output = output;
        }

        //[Theory]
        //[InlineData("", "wwwroot/A/another.js", "wwwroot/A/another.js")]
        //[InlineData("wwwroot/A/B/C/", "wwwroot/A/another.js", "../../another.js")]
        //public async void Can_Make_Relative_Path(string directoryPath, string someFilePath, string expected)
        //{
        //   // given a directory path, and also some file path, where they both share a common root directory,
        //   // can get the relative path from that directory to that file.

        //    SubPathInfo directory = SubPathInfo.Parse(directoryPath);
        //    SubPathInfo someFile = SubPathInfo.Parse(someFilePath);

        //    SubPathInfo relativePath = directory.GetRelativePathTo(someFile);
        //    Assert.Equal(relativePath.ToString(), expected);

        //}


    }
}