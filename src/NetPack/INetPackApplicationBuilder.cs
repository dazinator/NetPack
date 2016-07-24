using Microsoft.AspNetCore.Builder;
using NetPack.Pipeline;

namespace NetPack
{
    public interface INetPackApplicationBuilder : IApplicationBuilder
    {
        IPipeLine PipeLine { get;  }
    }
}