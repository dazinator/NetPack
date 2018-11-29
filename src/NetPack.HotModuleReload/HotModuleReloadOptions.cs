using System;
using System.Collections.Generic;
using System.Linq;

namespace NetPack.HotModuleReload
{

    public class WatchPatterns
    {
        public List<string> IncludePatterns { get; }
        public List<string> ExcludePatterns { get; }


        public WatchPatterns()
        {
            IncludePatterns = new List<string>();
            ExcludePatterns = new List<string>();
        }

        public WatchPatterns Include(string pattern)
        {
            string searchPattern = pattern;
            if (!pattern.StartsWith("/"))
            {
                searchPattern = "/" + pattern;
            }
            if (!IncludePatterns.Contains(searchPattern))
            {
                IncludePatterns.Add(searchPattern);
            }
            return this;
        }

        public WatchPatterns Exclude(string pattern)
        {
            string searchPattern = pattern;
            if (!pattern.StartsWith("/"))
            {
                searchPattern = "/" + pattern;
            }
            if (!ExcludePatterns.Contains(searchPattern))
            {
                ExcludePatterns.Add(searchPattern);
            }
            return this;
        }

    }
    public class HotModuleReloadOptions
    {
      

        public WatchPatterns WebRootWatchPatterns { get; }
        public WatchPatterns ContentRootWatchPatterns { get; }


        public HotModuleReloadOptions()
        {
            WebRootWatchPatterns = new WatchPatterns();
            ContentRootWatchPatterns = new WatchPatterns();

        }

        public HotModuleReloadOptions WatchWebRoot(Action<WatchPatterns> configure)
        {           
            configure?.Invoke(WebRootWatchPatterns);
            return this;
        }       

        public HotModuleReloadOptions WatchContentRoot(Action<WatchPatterns> configure)
        {           
            configure?.Invoke(ContentRootWatchPatterns);
            return this;
        }       

        /// <summary>
        /// A delay in milliseconds after a file change is detected, until the reload is signalled to the client browsers. 
        /// This is useful to deal with duplicate change token signalling causing multiple reload events to fire in very quick succession.
        /// During the delay, if multiple change tokens are signalled they are consolidated into a single reload event sent to the client.
        /// </summary>
        /// <returns></returns>
        public HotModuleReloadOptions SetDelay(int milliseconds)
        {
            Delay = milliseconds;
            return this;
        }

        public int Delay { get; set; }

    }
}
