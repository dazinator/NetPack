using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Pipes;

namespace NetPack.Tests.Pipes
{
    public class PipeTestBase
    {
        public SourceFile GivenASourceFile(string path, int length)
        {
            var subPath = SubPathInfo.Parse(path);
            var content = TestUtils.GenerateString(length) + Environment.NewLine + "//# sourceMappingURL=/" + subPath.ToString();
            //  _fileProvider.AddFile(subPath, content);
            var fileInfo = new SourceFile(new StringFileInfo(content, subPath.Name), subPath.Directory);
            return fileInfo;
        }

        public SourceFile GivenASourceFile(string path, Func<string> content)
        {
            var subPath = SubPathInfo.Parse(path);
            var fileContent = content();
            //  _fileProvider.AddFile(subPath, content);
            var fileInfo = new SourceFile(new StringFileInfo(fileContent, subPath.Name), subPath.Directory);
            return fileInfo;
        }

        protected async Task WhenFilesProcessedByPipe(Func<IPipe> pipeFactory, params SourceFile[] sourceFiles)
        {
            var sourceFilesList = new List<SourceFile>(sourceFiles);
            PipelineContext = new PipelineContext(sourceFilesList);
          
            Sut = pipeFactory();
            await Sut.ProcessAsync(PipelineContext, CancellationToken.None);
            PipelineContext.PrepareNextInputs();
        }

        protected SourceFile ThenTheOutputFileFromPipe(string filePath, Action<SourceFile> assertions)
        {
            var outputFile =
                PipelineContext.InputFiles.FirstOrDefault(
                    a => SubPathInfo.Parse(a.ToString()).Equals(SubPathInfo.Parse(filePath)));
            assertions(outputFile);
            return outputFile;
        }

        public IPipe Sut { get; set; }

        public PipelineContext PipelineContext { get; set; }

    }
}