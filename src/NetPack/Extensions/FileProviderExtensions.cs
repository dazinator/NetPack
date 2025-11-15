using System.IO;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipeline;
using System.Collections.Generic;
using System.Linq;
using Dazinator.Extensions.FileProviders;

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

        public static FileWithDirectory[] GetFiles(this IFileProvider fileProvider, PipeInput inputs)
        {
            var results = new Dictionary<string, FileWithDirectory>();
            var includes = inputs.GetIncludes().ToArray();
            var excludes = inputs.GetExcludes().ToArray();

            var files = fileProvider.Search(includes, excludes);
            // check if file already present? Multiple input patterns can match the same files.
            foreach (var file in files)
            {
                var path = $"{file.Item1}/{file.Item2.Name}";
                if (!results.ContainsKey(path))
                {
                    var item = new FileWithDirectory() { Directory = file.Item1, FileInfo = file.Item2 };
                    results.Add(path, item);
                }
            }

            return results.Values.ToArray();
        }      


    }
}