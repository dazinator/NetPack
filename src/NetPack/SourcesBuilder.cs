using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
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
            var dir = GetDirectoryPath(filePath);
            AddFile(file, dir);
            return this;
        }

        public void AddFile(IFileInfo file, string directory)
        {
            var sourceFile = new SourceFile(file, directory);
            SourceFiles.Add(sourceFile);
        }

        public List<SourceFile> SourceFiles { get; }


        protected virtual string GetDirectoryPath(string subPath)
        {
            // does the subpath contain directory portion?
            var indexOfLastSeperator = subPath.LastIndexOf('/');
            if (indexOfLastSeperator == -1 || indexOfLastSeperator == 0)
            {
                return string.Empty;
            }

            var dir = subPath.Substring(0, indexOfLastSeperator);
            return dir;

        }

    }
}