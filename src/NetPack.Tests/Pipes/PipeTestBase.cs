using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetPack.Pipeline;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Dazinator.Extensions.FileProviders.InMemory.Directory;
using Microsoft.AspNetCore.Http;
using Dazinator.Extensions.FileProviders;
using Dazinator.Extensions.FileProviders.InMemory;

namespace NetPack.Tests.Pipes
{
    public class PipeTestBase
    {

        private IDirectory _directory = new InMemoryDirectory();
        private IDirectory _sourcesdirectory = new InMemoryDirectory();

        public FileWithDirectory GivenAFileInfo(string path, int length)
        {
            PathStringUtils.GetPathAndFilename(path, out var directory, out var fileName);
            var fullPath = directory.Add($"/{fileName}");

            //var subPath = SubPathInfo.Parse(path);
            var content = TestUtils.GenerateString(length) + Environment.NewLine + "//# sourceMappingURL=" + fullPath;
            //  _fileProvider.AddFile(subPath, content);
            var fileInfo = new StringFileInfo(content, fileName);
            Directory.AddFile(directory, fileInfo);
            return new FileWithDirectory() { Directory = directory, FileInfo = fileInfo };
        }

        public FileWithDirectory GivenAFileInfo(string fileName, Func<string> content)
        {
            PathStringUtils.GetPathAndFilename(fileName, out var directory, out var name);
            //var subPath = SubPathInfo.Parse(fileName);
            var fileContent = content();
            //  _fileProvider.AddFile(subPath, content);
            var fileInfo = new StringFileInfo(fileContent, name);
            Directory.AddFile(directory, fileInfo);
            return new FileWithDirectory() { Directory = directory, FileInfo = fileInfo };
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

            var loggerFactory = LoggerFactory.Create(a => a.AddConsole());

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