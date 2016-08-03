using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using NetPack.Pipeline;

namespace NetPack.File
{
    public class NetPackPipelineFileProvider : INetPackPipelineFileProvider
    {

        private static char[] trimStartChars = new[] {'/'};

        private IPipeLine _pipeline;

        public NetPackPipelineFileProvider(IPipeLine pipeline)
        {
            _pipeline = pipeline;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var normalisedSubPath = subpath.TrimStart(trimStartChars);

            foreach (var file in _pipeline.Output.Files)
            {
                if (file.GetPath() == normalisedSubPath)
                {
                    return file.FileInfo;
                }
            }

            return new NotFoundFileInfo(subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var files = new List<IFileInfo>();
            var normalisedSubPath = subpath.TrimStart(trimStartChars);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var file in _pipeline.Output.Files)
            {
                if (file.Directory == normalisedSubPath)
                {
                    files.Add(file.FileInfo);
                }
            }

            if (files.Any())
            {
                return new EnumerableDirectoryContents(files);
            }

            return new NotFoundDirectoryContents();

        }

        public IChangeToken Watch(string filter)
        {
            // watching output files for changes not yet supported. 
            return NullChangeToken.Singleton;
            //throw new NotImplementedException();
        }
    }

    
}