using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace NetPack.Pipeline
{
    public class NetPackPipelineManagerFileProvider : INetPackPipelineFileProvider
    {

        private PipelineManager _pipelineManager;

        public NetPackPipelineManagerFileProvider(PipelineManager pipelineManager)
        {
            _pipelineManager = pipelineManager;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            foreach (var pipeline in _pipelineManager.PipeLines)
            {
                foreach (var file in pipeline.Output.Files)
                {
                    if (file.GetPath() == subpath)
                    {
                        return file.FileInfo;
                    }
                }
            }

            return new NotFoundFileInfo(subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var files = new List<IFileInfo>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var pipeline in _pipelineManager.PipeLines)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var file in pipeline.Output.Files)
                {
                    if (file.Directory == subpath)
                    {
                        files.Add(file.FileInfo);
                    }
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