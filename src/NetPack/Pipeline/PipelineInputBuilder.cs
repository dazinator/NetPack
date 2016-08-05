using System;
using Microsoft.Extensions.FileProviders;
using NetPack.File;

namespace NetPack.Pipeline
{
    public class PipelineInputBuilder
    {

        private readonly IFileProvider _sourceFileProvider;
        //private readonly IHostingEnvironment _hostingEnv;
        public PipelineInputBuilder(IFileProvider sourceFileProvider)
        {
            _sourceFileProvider = sourceFileProvider;
            // _sources = new Sources();
            Input = new PipelineInput(sourceFileProvider);
        }

        public PipelineInputBuilder Include(string filePath)
        {
            //todo: handle globbs too, 
            // however need to think about how we would handle detecting new files that appear matching the glob
            // for example if you create a new file at runtime, we should pick it up.
            // current implmentation listens to specific files only. 
            var subPath = SubPathInfo.Parse(filePath);
            if (!subPath.IsFile)
            {
                throw new ArgumentException("Only paths for specific files are currently supported. Globbs and directories cannot currently be used.");
            }
            var file = _sourceFileProvider.EnsureFile(filePath);
            var dir = GetDirectoryPath(filePath);
            return AddFile(file, dir);
        }

        public PipelineInputBuilder AddFile(IFileInfo file, string directory)
        {
            Input.AddFile(file, directory);
            return this;
        }

        public PipelineInput Input { get; set; }

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