using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace NetPack
{
    public class SourcesBuilder
    {

        private readonly IFileProvider _sourceFileProvider;
        private readonly IHostingEnvironment _hostingEnv;
       // private readonly Sources _sources;

        public SourcesBuilder(IHostingEnvironment hostingEnv) : this(hostingEnv.ContentRootFileProvider)
        {

        }

        public SourcesBuilder(IFileProvider sourceFileProvider)
        {
            _sourceFileProvider = sourceFileProvider;
           // _sources = new Sources();
            SourceFiles = new List<SourceFile>();
        }

        public SourcesBuilder Include(string filePath)
        {
            var file = _sourceFileProvider.EnsureFile(filePath);
            AddFile(file);
            return this;
        }

        public void AddFile(IFileInfo file)
        {
            var sourceFile = new SourceFile(file);
            SourceFiles.Add(sourceFile);
        }


        public List<SourceFile> SourceFiles { get; }
       
    }
}