using Microsoft.Extensions.FileProviders;

namespace NetPack.Pipeline
{
    public interface INetPackPipelineFileProvider : IFileProvider
    {
       // SourceFile GetSourceFile(string subpath);
    }
}