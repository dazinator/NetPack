using Microsoft.AspNetCore.NodeServices;

namespace NetPack
{
    public interface INetPackNodeServices : INodeServices
    {
        StringAsTempFile CreateStringAsTempFile(string content);

        string ProjectDir { get; }

    }
}