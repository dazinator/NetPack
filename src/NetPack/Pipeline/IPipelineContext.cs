using Dazinator.Extensions.FileProviders.InMemory.Directory;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;

namespace NetPack.Pipeline
{
    public interface IPipelineContext
    {

        PathString BaseRequestPath { get; }

        void AddGeneratedOutput(string directory, IFileInfo info);

        void AddSourceOutput(string directory, IFileInfo file);

        /// <summary>
        /// A directory where generated files are added.
        /// </summary>
        IDirectory GeneratedOutput { get; set; }

        /// <summary>
        /// A directory where source files that need to be served up should be added.
        /// </summary>
        IDirectory SourcesOutput { get; }

        IFileProvider FileProvider { get; set; }

        PathString GetRequestPath(string directory, IFileInfo fileInfo);       

    }
}