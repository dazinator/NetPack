using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipeline;

namespace NetPack
{
    public interface INetPackApplicationBuilder : IApplicationBuilder
    {
        IPipeLine Pipeline { get; }
        // IFileProvider PipelineFileProvider { get; }
    }
}