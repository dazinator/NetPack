using System.Collections.Generic;
using System.Linq;

namespace NetPack.BrowserReload
{
    public class BrowserReloadOptions
    {
        public List<string> WebRootWatchPatterns { get; }
        public List<string> ContentRootWatchPatterns { get; }


        public BrowserReloadOptions()
        {
            WebRootWatchPatterns = new List<string>();
            ContentRootWatchPatterns = new List<string>();
        }

        public BrowserReloadOptions WatchWebRoot(string pattern)
        {
            string searchPattern = pattern;
            if (!pattern.StartsWith("/"))
            {
                searchPattern = "/" + pattern;
            }
            if (!WebRootWatchPatterns.Contains(searchPattern))
            {
                WebRootWatchPatterns.Add(searchPattern);
            }
            return this;
        }

        public BrowserReloadOptions WatchContentRoot(string pattern)
        {
            string searchPattern = pattern;
            if (!pattern.StartsWith("/"))
            {
                searchPattern = "/" + pattern;
            }
            if (!ContentRootWatchPatterns.Contains(searchPattern))
            {
                ContentRootWatchPatterns.Add(searchPattern);
            }
            return this;
        }

        public IEnumerable<string> GetWebRootWatchPatterns()
        {
            return WebRootWatchPatterns.AsEnumerable();
        }

        public IEnumerable<string> GetContentRootWatchPatterns()
        {
            return ContentRootWatchPatterns.AsEnumerable();
        }

    }
}
