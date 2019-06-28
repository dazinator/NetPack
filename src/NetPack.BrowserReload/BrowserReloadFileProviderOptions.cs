using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.Linq;

namespace NetPack.BrowserReload
{
    public class BrowserReloadFileProviderOptions
    {

        public BrowserReloadFileProviderOptions(IFileProvider fileProvider)
        {
            WatchPatterns = new List<string>();
            FileProvider = fileProvider;
            // ContentRootWatchPatterns = new List<string>();
        }

        public IFileProvider FileProvider { get; set; }
        public List<string> WatchPatterns { get; }
        public IEnumerable<string> GetWatchPatterns()
        {
            return WatchPatterns.AsEnumerable();
        }

        public BrowserReloadFileProviderOptions Watch(string pattern)
        {
            string searchPattern = pattern;
            if (!pattern.StartsWith("/"))
            {
                searchPattern = "/" + pattern;
            }
            if (!WatchPatterns.Contains(searchPattern))
            {
                WatchPatterns.Add(searchPattern);
            }
            return this;
        }

    }
}
