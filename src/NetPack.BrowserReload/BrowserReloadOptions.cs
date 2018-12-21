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

        /// <summary>
        /// A delay in milliseconds after a file change is detected, until the reload is signalled to the client browsers. 
        /// This is useful to deal with duplicate change token signalling causing multiple reload events to fire in very quick succession.
        /// During the delay, if multiple change tokens are signalled they are consolidated into a single reload event sent to the client.
        /// </summary>
        /// <returns></returns>
        public BrowserReloadOptions SetDelay(int milliseconds)
        {
            Delay = milliseconds;
            return this;
        }

        public int Delay { get; set; }

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
