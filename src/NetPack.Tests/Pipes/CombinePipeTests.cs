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
            ThenTheProcessedOutputDirectoryFile("mybundle.js", Assert.NotNull);

            // The original, source files. should not be in the processed output directory.
            ThenTheProcessedOutputDirectoryFile("js/red.js", Assert.Null);
            ThenTheProcessedOutputDirectoryFile("js/green.js", Assert.Null);
            ThenTheProcessedOutputDirectoryFile("js/blue.js", Assert.Null);           

            // The combined file should not have any of the source mapping urls that were present in the original input files.
            ThenTheProcessedOutputDirectoryFile("mybundle.js", (combinedFile) =>
            {
                var content = combinedFile.ReadAllContent();
                Assert.DoesNotContain("//# sourceMappingURL=/" + "js/red.js.map", content);
                Assert.DoesNotContain("//# sourceMappingURL=/" + "js/green.js.map", content);
                Assert.DoesNotContain("//# sourceMappingURL=/" + "js/blue.js.map", content);
            });

        
            if (sourceMapMode != SourceMapMode.None)
            {
                // The combined file should have a source mapping url, which points to its own index source map file.
                ThenTheProcessedOutputDirectoryFile("mybundle.js", (combinedFile) =>
                {
                    var content = combinedFile.ReadAllContent();
                    Assert.Contains("//# sourceMappingURL=" + "mybundle.js.map", content);
                });

                // There should be an index source map.
                ThenTheProcessedOutputDirectoryFile("mybundle.js.map", Assert.NotNull);

                // Because source maps are enabled, the processor should place the source files into the sources directory which will allow them to be
                // served up to the browser.
                ThenTheSourcesDirectoryFile("js/red.js", Assert.NotNull);
                ThenTheSourcesDirectoryFile("js/green.js", Assert.NotNull);
                ThenTheSourcesDirectoryFile("js/blue.js", Assert.NotNull);
            }
            else
            {
                // The combined file should not have any source mapping url.
                ThenTheProcessedOutputDirectoryFile("mybundle.js", (combinedFile) =>
                {
                    var content = combinedFile.ReadAllContent();
                    Assert.DoesNotContain("//# sourceMappingURL=/" + "mybundle.js.map", content);
                });

                // There should not be an index source map.
                ThenTheProcessedOutputDirectoryFile("mybundle.js.map", Assert.Null);

                // Because source maps are not enabled, the processor should not place the source files into the sources directory, as they will not be served up to the 
                // browser using the net pack integrated webroot file provider.
                ThenTheSourcesDirectoryFile("js/red.js", Assert.Null);
                ThenTheSourcesDirectoryFile("js/green.js", Assert.Null);
                ThenTheSourcesDirectoryFile("js/blue.js", Assert.Null);
            }

        }

    }


}