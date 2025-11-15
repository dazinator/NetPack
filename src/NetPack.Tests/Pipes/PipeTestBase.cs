using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.Extensions.FileProviders;
using Dazinator.Extensions.FileProviders.InMemory;
using Dazinator.Extensions.FileProviders.InMemory.Directory;
using NetPack.Pipeline;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NetPack.Utils;

namespace NetPack.Tests.Pipes
{
    public class PipeTestBase
    {

        private IDirectory _directory = new InMemoryDirectory();
        private IDirectory _sourcesdirectory = new InMemoryDirectory();

        public FileWithDirectory GivenAFileInfo(string path, int length)
        {
            var subPath = SubPathInfo.Parse(path);
            var content = TestUtils.GenerateString(length) + Environment.NewLine + "//# sourceMappingURL=/" + subPath.ToString();
            var fileInfo = new StringFileInfo(content, subPath.Name);
            Directory.AddFile(subPath.Directory, fileInfo);
            return new FileWithDirectory() { Directory = subPath.Directory, FileInfo = fileInfo };
        }

        public FileWithDirectory GivenAFileInfo(string fileName, Func<string> content)
        {
            var subPath = SubPathInfo.Parse(fileName);
            var fileContent = content();
            var fileInfo = new StringFileInfo(fileContent, subPath.Name);
            Directory.AddFile(subPath.Directory, fileInfo);
            return new FileWithDirectory() { Directory = "/" + subPath.Directory, FileInfo = fileInfo };
        }

        protected async Task WhenFilesProcessedByPipe(Func<IPipe> pipeFactory, params FileWithDirectory[] files)
        {
            //  var sourceFilesList = new List<IFileInfo>(files);
            var provider = new InMemoryFileProvider(Directory);
            //  PipelineContext = new PipelineContext(provider, _sourcesdirectory);
            var input = new PipeInput();
            foreach (var item in files)
            {
                input.AddInclude(item.UrlPath);
            }
            Sut = pipeFactory();
            var loggerFactory =  LoggerFactory.Create(c=>c.AddConsole());

            var pipeContext = new PipeProcessor(input, Sut, loggerFactory.CreateLogger<PipeProcessor>());
            var pipes = new List<PipeProcessor>() { pipeContext };

            Pipeline = new Pipeline.Pipeline(provider, pipes, null, _sourcesdirectory, loggerFactory.CreateLogger<Pipeline.Pipeline>());
            await Pipeline.ProcessPipesAsync(pipes, CancellationToken.None);

        }

        protected IFileInfo ThenTheProcessedOutputDirectoryFile(string filePath, Action<IFileInfo> assertions)
        {
            var outputFile = Pipeline.Context.GeneratedOutput.GetFile(filePath);
            //_directory.FirstOrDefault(
            //    a => SubPathInfo.Parse(a.ToString()).Equals(SubPathInfo.Parse(filePath)));
            assertions(outputFile?.FileInfo);
            return outputFile?.FileInfo;
        }

        protected IFileInfo ThenTheSourcesDirectoryFile(string filePath, Action<IFileInfo> assertions)
        {
            var sourceFile = Pipeline.Context.SourcesOutput.GetFile(filePath);
            //_directory.FirstOrDefault(
            //    a => SubPathInfo.Parse(a.ToString()).Equals(SubPathInfo.Parse(filePath)));
            assertions(sourceFile?.FileInfo);
            return sourceFile?.FileInfo;
        }

        public IPipe Sut { get; set; }

        public IPipeLine Pipeline { get; set; }
        public IDirectory Directory { get => _directory; set => _directory = value; }
    }
}