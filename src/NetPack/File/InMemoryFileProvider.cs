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
            var dir = SubPathInfo.Parse(subpath);

            foreach (var file in Files)
            {
                if (file.Key.IsInDirectory(dir))
                {
                    files.Add(file.Value);
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
            bool isComposite = subPath.IsPattern || string.IsNullOrWhiteSpace(subPath.FileName);

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
            var file = new StringFileInfo(contents, subpath.FileName);
            Files.Add(subpath, file);
        }

        public void AddFile(string subpath, string contents)
        {
            var path = SubPathInfo.Parse(subpath);
            var file = new StringFileInfo(contents, path.FileName);
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