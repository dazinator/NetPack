using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetPack.Extensions
{
    public static class PipelineInputExtensions
    {
        public static FileWithDirectory[] GetInputFiles(this PipelineInput inputs, IFileProvider fileProvider)
        {
            var results = new Dictionary<string, FileWithDirectory>();
            foreach (var input in inputs.IncludeList)
            {
                var files = fileProvider.Search(input);
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

            }
            return results.Values.ToArray();
        }
    }
}
