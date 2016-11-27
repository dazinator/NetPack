using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Pipes;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.Extensions.FileProviders;

namespace NetPack.Tests.Pipes
{
    public class PipeTestBase
    {

        private IDirectory _directory = new InMemoryDirectory();

        public IFileInfo GivenAFileInfo(string path, int length)
        {
            var subPath = SubPathInfo.Parse(path);
            var content = TestUtils.GenerateString(length) + Environment.NewLine + "//# sourceMappingURL=/" + subPath.ToString();
            //  _fileProvider.AddFile(subPath, content);
            var fileInfo = new StringFileInfo(content, subPath.Name);
            _directory.AddFile(subPath.Directory, fileInfo);
            return fileInfo;
        }

        public IFileInfo GivenAFileInfo(string fileName, Func<string> content)
        {
            var subPath = SubPathInfo.Parse(fileName);
            var fileContent = content();
            //  _fileProvider.AddFile(subPath, content);
            var fileInfo = new StringFileInfo(fileContent, subPath.Name);
            _directory.AddFile(subPath.Directory, fileInfo);
            return fileInfo;
        }

        protected async Task WhenFilesProcessedByPipe(Func<IPipe> pipeFactory, params IFileInfo[] files)
        {
            var sourceFilesList = new List<IFileInfo>(files);
            var provider = new InMemoryFileProvider(_directory);
            PipelineContext = new PipelineContext(provider, _directory, "");
            Sut = pipeFactory();
            await Sut.ProcessAsync(PipelineContext, files, CancellationToken.None);
        }

        protected IFileInfo ThenTheOutputFileFromPipe(string filePath, Action<IFileInfo> assertions)
        {
            var outputFile = _directory.GetFile(filePath);
                //_directory.FirstOrDefault(
                //    a => SubPathInfo.Parse(a.ToString()).Equals(SubPathInfo.Parse(filePath)));
            assertions(outputFile.FileInfo);
            return outputFile.FileInfo;
        }

        public IPipe Sut { get; set; }

        public PipelineContext PipelineContext { get; set; }

    }
}