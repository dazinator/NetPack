using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetPack.Pipes;
using Xunit;

namespace NetPack.Tests.Pipes
{
    public class CombinePipeTests : PipeTestBase
    {

        [InlineData(SourceMapMode.None, new string[] { "js/red.js", "js/green.js", "js/blue.js" })]
        [InlineData(SourceMapMode.Inline, new string[] { "js/red.js", "js/green.js", "js/blue.js" })]
        [Theory]
        public async Task Combines_Javascript_Files_With_Source_Maps(SourceMapMode sourceMapMode, params string[] jsFilePaths)
        {

            // Given some javascript files, that have source mapping url declarations, and their corresponding .map files.
            List<FileWithDirectory> jsFiles = new List<FileWithDirectory>();
            foreach (var jsFilePath in jsFilePaths)
            {
                var jsFileMapPath = jsFilePath + ".map";
                var jsFile = GivenAFileInfo(jsFilePath, () => TestUtils.GenerateString(1000) + Environment.NewLine + "//# sourceMappingURL=/" + jsFileMapPath);
                var jsFileMap = GivenAFileInfo(jsFileMapPath, () => "{some: true, json: true}");

                jsFiles.Add(jsFile);
            }

            // When they are processed by the following combine pipe
            await WhenFilesProcessedByPipe(() =>
            {
                var options = new JsCombinePipeOptions()
                {
                    OutputFilePath = "mybundle.js",
                    SourceMapMode = sourceMapMode
                };
                return new JsCombinePipe(options);
            }, jsFiles.ToArray());


            // The the combined file should be output from the pipe.
            ThenTheOutputFileFromPipe("mybundle.js", Assert.NotNull);

            // The original, individual files. should not be output from the pipe.
            ThenTheOutputFileFromPipe("js/red.js", Assert.Null);
            ThenTheOutputFileFromPipe("js/green.js", Assert.Null);
            ThenTheOutputFileFromPipe("js/blue.js", Assert.Null);

            // The combined file should not have any of the source mapping urls present in the input files.
            ThenTheOutputFileFromPipe("mybundle.js", (combinedFile) =>
            {
                var content = combinedFile.ReadAllContent();
                Assert.DoesNotContain("//# sourceMappingURL=/" + "js/red.js.map", content);
                Assert.DoesNotContain("//# sourceMappingURL=/" + "js/green.js.map", content);
                Assert.DoesNotContain("//# sourceMappingURL=/" + "js/blue.js.map", content);
            });

            // The combined file should have a source mapping url, which points to its own map file, containing the index source map.

            if (sourceMapMode != SourceMapMode.None)
            {
                ThenTheOutputFileFromPipe("mybundle.js", (combinedFile) =>
                {
                    var content = combinedFile.ReadAllContent();
                    Assert.Contains("//# sourceMappingURL=/" + "mybundle.js.map", content);
                });

                // The index map file should be output from the pipe.
                ThenTheOutputFileFromPipe("mybundle.js.map", Assert.NotNull);
            }
            else
            {
                ThenTheOutputFileFromPipe("mybundle.js", (combinedFile) =>
                {
                    var content = combinedFile.ReadAllContent();
                    Assert.DoesNotContain("//# sourceMappingURL=/" + "mybundle.js.map", content);
                });

                // There shoudl be no index map file produce from the pipe.
                ThenTheOutputFileFromPipe("mybundle.js.map", Assert.Null);
            }

        }

    }


}