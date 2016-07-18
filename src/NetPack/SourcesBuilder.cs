using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace NetPack
{
    public class SourcesBuilder
    {

        private readonly IFileProvider _sourceFileProvider;
        private readonly IHostingEnvironment _hostingEnv;
        private readonly Sources _sources;

        public SourcesBuilder(IHostingEnvironment hostingEnv) : this(hostingEnv.ContentRootFileProvider)
        {

        }

        public SourcesBuilder(IFileProvider sourceFileProvider)
        {
            _sourceFileProvider = sourceFileProvider;
            _sources = new Sources();
        }

        public SourcesBuilder Include(string filePath)
        {
            var file = _sourceFileProvider.EnsureFile(filePath);
            _sources.AddFile(file);
            return this;
        }

        public Sources Sources => _sources;
    }
}