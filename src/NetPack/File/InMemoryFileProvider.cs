using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace NetPack.File
{
    public class InMemoryFileProvider : IFileProvider
    {

        private readonly ConcurrentDictionary<SubPathInfo, IChangeToken> _matchInfoCache = new ConcurrentDictionary<SubPathInfo, IChangeToken>();

        public InMemoryFileProvider()
        {
            Files = new Dictionary<SubPathInfo, StringFileInfo>();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var subpathInfo = SubPathInfo.Parse(subpath);
            return GetFileInfo(subpathInfo);
        }

        public IFileInfo GetFileInfo(SubPathInfo subpath)
        {
            if (!Files.ContainsKey(subpath))
            {
                return new NotFoundFileInfo(subpath.ToString());
            }

            return Files[subpath];
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var files = new List<IFileInfo>();
            var parentDirectory = SubPathInfo.Parse(subpath);

            foreach (var folder in Files.Select(a => a.Key.GetDescendentFolderNameFrom(parentDirectory)).Distinct())
            {
                if (folder != null)
                {
                    files.Add(new NetPackDirectoryInfo(folder));
                }
            }
            //var childFolders = _pipeline.Output.Files.Where(a => subPathInfo.IsChildDirectory(a.Directory)).Select(a => a.Directory.ToString()).Distinct();
            var childFiles = Files.Where(a => a.Key.IsInSameDirectory(parentDirectory));

            //foreach (var folder in childFolders)
            //{
            //    files.Add(new NetPackDirectoryInfo(folder));
            //}

            foreach (var file in childFiles)
            {
                files.Add(file.Value);
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



            //var files = new List<IFileInfo>();
            //var dir = SubPathInfo.Parse(subpath);

            //foreach (var file in Files)
            //{
            //    if (file.Key.IsInSameDirectory(dir))
            //    {
            //        files.Add(file.Value);
            //    }
            //}

            //if (files.Any())
            //{
            //    return new EnumerableDirectoryContents(files);
            //}

            //return new NotFoundDirectoryContents();

        }

        public IChangeToken Watch(string filter)
        {

            if (filter == null)
            {
                return NullChangeToken.Singleton;
            }

            var subPath = SubPathInfo.Parse(filter);
            IChangeToken existing;
            if (this._matchInfoCache.TryGetValue(subPath, out existing))
            {
                return existing;
            }

            // var filesToWatch = new List<StringFileInfo>();

            var fileTokens = new List<IChangeToken>();
            bool isComposite = subPath.IsPattern || string.IsNullOrWhiteSpace(subPath.Name);

            foreach (var file in Files)
            {
                if (file.Key.IsMatch(subPath))
                {
                    // ensure token for the file.
                    var key = file.Key;
                    var fileToken = GetOrAddChangeToken(key, new InMemoryChangeToken());
                    fileTokens.Add(fileToken);

                    if (!isComposite)
                    {
                        // stop searching if single file.
                        break;
                    }
                }
            }

            // if composite token, add it to cache in case someone watches with same pattern then they get the same token.
            IChangeToken resultToken;
            if (!fileTokens.Any())
            {
                resultToken = NullChangeToken.Singleton;
            }
            // ensure composite token added.
            else if (fileTokens.Count == 1)
            {
                resultToken = fileTokens.Single(); 
            }
            else
            {
                // wrap the individual file tokens in a composite.
                var allFileTokens = fileTokens.AsEnumerable<IChangeToken>().ToList();
                resultToken = new CompositeChangeToken(allFileTokens);
                GetOrAddChangeToken(subPath, resultToken);
            }

            if (isComposite)
            {
                GetOrAddChangeToken(subPath, resultToken);
            }
            return resultToken;


        }

        private IChangeToken GetOrAddChangeToken(SubPathInfo key, IChangeToken token)
        {
            IChangeToken fileToken;
            if (_matchInfoCache.TryGetValue(key, out fileToken))
            {
                // return existing token for this file path.
            }
            else
            {
                // var newToken = new InMemoryChangeToken();
                fileToken = _matchInfoCache.GetOrAdd(key, token);
            }
            return fileToken;
        }

        public void AddFile(SubPathInfo subpath, string contents)
        {
            var file = new StringFileInfo(contents, subpath.Name);
            Files.Add(subpath, file);
        }

        public void AddFile(string subpath, string contents)
        {
            var path = SubPathInfo.Parse(subpath);
            var file = new StringFileInfo(contents, path.Name);
            Files.Add(path, file);
        }

        public Dictionary<SubPathInfo, StringFileInfo> Files { get; set; }

        public void UpdateFile(SubPathInfo path, StringFileInfo stringFileInfo)
        {
            //var existingFile = GetFileInfo(path);
            Files[path] = stringFileInfo;

            IChangeToken fileToken;
            if (_matchInfoCache.TryGetValue(path, out fileToken))
            {
                var inMemory = fileToken as InMemoryChangeToken;
                if (inMemory != null)
                {
                    // return existing token for this file path.
                    inMemory.HasChanged = true;
                    inMemory.RaiseCallback(stringFileInfo);
                }
            }
        }
    }
}