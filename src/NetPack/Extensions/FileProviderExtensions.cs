using System.IO;
using Microsoft.Extensions.FileProviders;
using ReflectionBridge.Extensions;

namespace NetPack
{
    // ReSharper disable once CheckNamespace
    // Extension method put in root namespace for discoverability purposes.
    public static class FileProviderExtensions
    {
        public static IFileInfo EnsureFile(this IFileProvider fileProvider, string path)
        {
            var file = fileProvider.GetFileInfo(path);
            if (file.Exists)
            {
                if (file.IsDirectory)
                {
                    throw new FileNotFoundException($"The specified path was a directory, but expected a file path: {0}", path);
                }
                return file;
            }
            else
            {
                throw new FileNotFoundException($"No such file exists: {0}", path);
            }
        }

        public static string ReadAllContent(this IFileInfo fileInfo)
        {
            using (var reader = new StreamReader(fileInfo.CreateReadStream()))
            {
                return reader.ReadToEnd();
            }
        }
        
  


    }
}