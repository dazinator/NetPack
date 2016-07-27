using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace NetPack.Utils
{
    public interface IEmbeddedResourceProvider
    {
        IFileInfo GetResourceFile(Assembly assy, string path);
    }
}