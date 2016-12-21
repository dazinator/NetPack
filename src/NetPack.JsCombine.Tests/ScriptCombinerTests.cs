using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Xunit;
using Xunit.Abstractions;
using Dazinator.AspNet.Extensions.FileProviders;
using NetPack.Tests;

namespace NetPack.JsCombine.Tests
{
    public class ScriptCombinerTests
    {
        private ITestOutputHelper _output;
        private InMemoryFileProvider _fileProvider;
        private List<Tuple<int, CombinedScriptInfo>> _scriptInfoForAssert;
        private MemoryStream _combinedFileOutputStream;

        public ScriptCombinerTests(ITestOutputHelper output)
        {
            _output = output;
            _fileProvider = new InMemoryFileProvider();
        }

        [Fact]
        public void When_Files_Are_Combined_Source_Mapping_Urls_Are_Removed()
        {
            // set up a couple of in memory files, for fileA and fileB
            // that both have source mapping url comments.

            var fileA = GivenFileThatHasSourceMappingUrl("wwwroot/fileA.js", 8000);
            var fileB = GivenFileThatHasSourceMappingUrl("wwwroot/fileB.js", 1000);

            WhenCombined(fileA, fileB);

            ThenLineNumberInformationShouldBeCorrect();
            ThenCombinedOutputShouldntContainSourceMappingUrls();

        }

        public ScriptCombiner Sut { get; set; }

        private IFileInfo GivenFileThatHasSourceMappingUrl(string path, int length)
        {
            var subPathInfo = SubPathInfo.Parse(path);
            var subPath = subPathInfo.ToString();
            var fileName = subPathInfo.Name;
            var mapFileName = subPathInfo + ".map";


            var content = TestUtils.GenerateString(length) + Environment.NewLine + "//# sourceMappingURL=/" + mapFileName;
            var addedFileInfo = _fileProvider.Directory.AddFile(subPath, new StringFileInfo(content, fileName)).FileInfo;
            //var fileInfo = _fileProvider.GetFileInfo(subPath);
            return addedFileInfo;
        }

        private void ThenCombinedOutputShouldntContainSourceMappingUrls()
        {
            // Checks that the combined output does not have source mapping url declarations.
            // but wuthout sourcemapping urls.
            _combinedFileOutputStream.Position = 0;
            var outputStreamReader = new StreamReader(_combinedFileOutputStream);
            var text = outputStreamReader.ReadToEnd();
            _output.WriteLine(text);
            Assert.DoesNotContain("//# sourceMappingURL=", text);
        }

        private void ThenLineNumberInformationShouldBeCorrect()
        {
            foreach (var combinedScriptInfo in _scriptInfoForAssert)
            {
                // just asserting that the actual linecount in the file, matches the linecount that
                // that the script combiner reported for the file when the file was added. 
                Assert.Equal(combinedScriptInfo.Item1, combinedScriptInfo.Item2.LineCount);
            }
        }

        private void WhenCombined(params IFileInfo[] files)
        {

            _combinedFileOutputStream = new MemoryStream();
            _scriptInfoForAssert = new List<Tuple<int, CombinedScriptInfo>>();

            Sut = new ScriptCombiner();

            foreach (var file in files)
            {
                // We will 
                int lineCount = 0;
                using (var testReader = new StreamReader(file.CreateReadStream()))
                {
                    while (!testReader.EndOfStream)
                    {
                        lineCount = lineCount + 1;
                        //  long pos = testReader.GetActualPosition();
                        var lineText = testReader.ReadLine();
                        // _linePositionsForAssert.Add(lineCount, pos);
                    }
                }

                using (var fileStream = file.CreateReadStream())
                {
                    using (var writer = new StreamWriter(_combinedFileOutputStream, Encoding.UTF8, 1024, true))
                    {
                        var scriptInfo = Sut.AddScript(fileStream, writer).Result;
                        _scriptInfoForAssert.Add(new Tuple<int, CombinedScriptInfo>(lineCount, scriptInfo));
                    }

                }
            }

        }


    }
}