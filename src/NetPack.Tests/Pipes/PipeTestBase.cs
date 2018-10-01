using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.RequireJs;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.Extensions.FileProviders;
using Polly;
using Microsoft.Extensions.Logging;

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
            //  _fileProvider.AddFile(subPath, content);
            var fileInfo = new StringFileInfo(content, subPath.Name);
            _directory.AddFile(subPath.Directory, fileInfo);
            return new FileWithDirectory() { Directory = subPath.Directory, FileInfo = fileInfo };
        }

        public FileWithDirectory GivenAFileInfo(string fileName, Func<string> content)
        {
            var subPath = SubPathInfo.Parse(fileName);
            var fileContent = content();
            //  _fileProvider.AddFile(subPath, content);
            var fileInfo = new StringFileInfo(fileContent, subPath.Name);
            _directory.AddFile(subPath.Directory, fileInfo);
            return new FileWithDirectory() { Directory = "/" + subPath.Directory, FileInfo = fileInfo };
        }

        protected async Task WhenFilesProcessedByPipe(Func<IPipe> pipeFactory, params FileWithDirectory[] files)
        {
            //  var sourceFilesList = new List<IFileInfo>(files);
            var provider = new InMemoryFileProvider(_directory);
            //  PipelineContext = new PipelineContext(provider, _sourcesdirectory);
            var input = new PipelineInput();
            foreach (var item in files)
            {
                input.AddInclude(item.UrlPath);
            }
            Sut = pipeFactory();
            var pipeContext = new PipeContext(input, Sut, new LoggerFactory().AddConsole().CreateLogger<PipeContext>());
            var pipes = new List<PipeContext>() { pipeContext };

            Pipeline = new Pipeline.Pipeline(provider, pipes, null, _sourcesdirectory);
            await Pipeline.ProcessPipesAsync(pipes, CancellationToken.None);

        }

        protected IFileInfo ThenTheProcessedOutputDirectoryFile(string filePath, Action<IFileInfo> assertions)
        {
            var outputFile = Pipeline.GeneratedOutputDirectory.GetFile(filePath);
            //_directory.FirstOrDefault(
            //    a => SubPathInfo.Parse(a.ToString()).Equals(SubPathInfo.Parse(filePath)));
            assertions(outputFile?.FileInfo);
            return outputFile?.FileInfo;
        }

        protected IFileInfo ThenTheSourcesDirectoryFile(string filePath, Action<IFileInfo> assertions)
        {
            var sourceFile = Pipeline.SourcesOutputDirectory.GetFile(filePath);
            //_directory.FirstOrDefault(
            //    a => SubPathInfo.Parse(a.ToString()).Equals(SubPathInfo.Parse(filePath)));
            assertions(sourceFile?.FileInfo);
            return sourceFile?.FileInfo;
        }

        public IPipe Sut { get; set; }

        public IPipeLine Pipeline { get; set; }

    }
}