using System;
using System.Threading.Tasks;
using NetPack.Pipes;
using Xunit;

namespace NetPack.Tests.Pipes
{
    public class CombinePipeTests : PipeTestBase
    {


        [Fact]
        public async Task Combines_Javascript_Files_With_Source_Maps()
        {

            // Given some files with source mapping url declarations.
            var jsFile1 = GivenASourceFile("js/red.js", () => TestUtils.GenerateString(1000) + Environment.NewLine + "//# sourceMappingURL=/" + "js/red.js.map");
            var jsFile2 = GivenASourceFile("js/green.js", () => TestUtils.GenerateString(1000) + Environment.NewLine + "//# sourceMappingURL=/" + "js/green.js.map");
            var jsFile3 = GivenASourceFile("js/blue.js", () => TestUtils.GenerateString(1000) + Environment.NewLine + "//# sourceMappingURL=/" + "js/blue.js.map");

            // When they are processed by the following combine pipe
            await WhenFilesProcessedByPipe(() =>
            {
                var options = new CombinePipeOptions()
                {
                    EnableJavascriptBundle = true,
                    CombinedJsFileName = "mybundle.js",
                    EnableIndexSourceMap = true
                };
                return new JsCombinePipe(options);
            }, jsFile1, jsFile2, jsFile3);


            // The the combined file should be output from the pipe.
            ThenTheOutputFileFromPipe("mybundle.js", Assert.NotNull);

            // The original, individual files. should not be output from the pipe.
            ThenTheOutputFileFromPipe("js/red.js", Assert.Null);
            ThenTheOutputFileFromPipe("js/green.js", Assert.Null);
            ThenTheOutputFileFromPipe("js/blue.js", Assert.Null);

            // The combined file should not have any of the source mapping urls present in the input files.
            ThenTheOutputFileFromPipe("mybundle.js", (combinedFile) =>
            {
                var content = combinedFile.FileInfo.ReadAllContent();
                Assert.DoesNotContain("//# sourceMappingURL=/" + "js/red.js.map", content);
                Assert.DoesNotContain("//# sourceMappingURL=/" + "js/green.js.map", content);
                Assert.DoesNotContain("//# sourceMappingURL=/" + "js/blue.js.map", content);
            });

            // The combined file should have a single source mapping url, which is the index map.
            ThenTheOutputFileFromPipe("mybundle.js", (combinedFile) =>
            {
                var content = combinedFile.FileInfo.ReadAllContent();
                Assert.Contains("//# sourceMappingURL=/" + "mybundle.js.map", content);
            });

            // The index map file should be output from the pipe.
            ThenTheOutputFileFromPipe("mybundle.js.map", Assert.NotNull);

        }

        [Fact]
        public async Task Allows_Non_Js_Files_To_Flow_Through()
        {

            // Given a mixture of js files and other file types.
            var jsFile1 = GivenASourceFile("js/red.js", () => TestUtils.GenerateString(1000) + Environment.NewLine + "//# sourceMappingURL=/" + "js/red.js.map");
            var nonJsFile1 = GivenASourceFile("other/green.other", () => TestUtils.GenerateString(1000));
            var nonJsFile2 = GivenASourceFile("blue.other", () => TestUtils.GenerateString(1000) + Environment.NewLine);

            // When they are processed to combine JS files
            await WhenFilesProcessedByPipe(() =>
            {
                var options = new CombinePipeOptions()
                {
                    EnableJavascriptBundle = true,
                    CombinedJsFileName = "mybundle.js",
                    EnableIndexSourceMap = true
                };
                return new JsCombinePipe(options);
            }, jsFile1, nonJsFile1, nonJsFile2);


            // The the combined file should be output from the pipe.
            ThenTheOutputFileFromPipe("mybundle.js", Assert.NotNull);

            // The original, now combined file should not be output from the pipe.
            ThenTheOutputFileFromPipe("js/red.js", Assert.Null);

            // The files that aren't js files so arent included in the js bundle should be output from the pipe
            ThenTheOutputFileFromPipe("other/green.other", Assert.NotNull);
            ThenTheOutputFileFromPipe("blue.other", Assert.NotNull);

        }

    }


}