using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace NetPack.Utils
{
    public class EmbeddedResourceProvider : IEmbeddedResourceProvider
    {
        private static ConcurrentDictionary<Assembly, IFileProvider> _fileProviders =
            new ConcurrentDictionary<Assembly, IFileProvider>();

        public IFileInfo GetResourceFile(Assembly assy, string path)
        {
            var fileProvider = _fileProviders.GetOrAdd(assy, (a) =>
            {
                var namespaceForFileProvider = assy.GetName().Name;
                var provider = new EmbeddedFileProvider(a, namespaceForFileProvider);
                return provider;
            });

            return fileProvider.GetFileInfo(path);
        }

    }

}