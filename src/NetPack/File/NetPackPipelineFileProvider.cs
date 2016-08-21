using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using NetPack.Pipeline;

namespace NetPack.File
{
    public class NetPackPipelineFileProvider : INetPackPipelineFileProvider
    {

        private static char[] trimStartChars = new[] { '/' };

        // private string _webRootFolder;

        private IPipeLine _pipeline;

        public NetPackPipelineFileProvider(IPipeLine pipeline)
        {
            _pipeline = pipeline;
            // _webRootFolder = webrootFolder;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var normalisedSubPath = SubPathInfo.Parse(subpath);

            foreach (var file in _pipeline.Output.Files)
            {
                if (file.ContentPathInfo.IsMatch(normalisedSubPath))
                {
                    return file.FileInfo;
                }
            }

            return new NotFoundFileInfo(subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {

            var files = new List<IFileInfo>();
            var parentDirectory = SubPathInfo.Parse(subpath);


            foreach (var folder in _pipeline.Output.Files.Select(a => a.WebRootPathInfo.GetDescendentFolderNameFrom(parentDirectory)).Distinct())
            {
                if (folder != null)
                {
                    files.Add(new NetPackDirectoryInfo(folder));
                }
            }
            //var childFolders = _pipeline.Output.Files.Where(a => subPathInfo.IsChildDirectory(a.Directory)).Select(a => a.Directory.ToString()).Distinct();
            var childFiles = _pipeline.Output.Files.Where(a => a.WebRootPathInfo.Directory == parentDirectory.Directory);

            //foreach (var folder in childFolders)
            //{
            //    files.Add(new NetPackDirectoryInfo(folder));
            //}

            foreach (var file in childFiles)
            {
                files.Add(file.FileInfo);
            }

            //var files = _pipeline.Output.Files.Where(a=>a.FullPath.IsChildFileOrDirectory(subPathInfo))

            //// var normalisedSubPath = subpath.TrimStart(trimStartChars);
            //if (!subPathInfo.IsEmpty)
            //{

            //   // var childFiles = 
            //    int directoryLevel = subPathInfo.GetDirectoryLevel();

            //    foreach (var file in _pipeline.Output.Files)
            //    {

            //        if (subPathInfo.IsChildFileOrDirectory(file.GetPath()))
            //        {

            //        }
            //        if (file.Directory == subPathInfo.Directory)
            //        {
            //            files.Add(file.FileInfo);
            //        }
            //    }
            //}
            //else
            //{
            //    // return all files and directories in root directory
            //    //  var directories = new Dictionary<SubPathInfo, IFileInfo>
            //    var filesInRoot = _pipeline.Output.Files.Where(a => string.IsNullOrEmpty(a.Directory));

            //    var dirsInRoot =
            //        _pipeline.Output.Files.Select(a => a.Directory.Split('/').FirstOrDefault()).Distinct();
            //    //'(d => d == ' /' || d == '\\') == 0)
            //    //  .Select(a => a.Directory)
            //    // .Distinct();


            //    //  || (a => a.Directory).Where(b=>b. ..Distinct()
            //    foreach (var dir in dirsInRoot)
            //    {
            //        files.Add(new NetPackDirectoryInfo(dir));
            //    }
            //    foreach (var file in filesInRoot)
            //    {
            //        files.Add(file.FileInfo);
            //    }

            //}

            // ReSharper disable once LoopCanBeConvertedToQuery


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