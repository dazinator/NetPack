using Microsoft.Extensions.FileProviders;

namespace NetPack.File
{
    public interface INetPackPipelineFileProvider : IFileProvider
    {
       // SourceFile GetSourceFile(string subpath);
    }
}