using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetPack.BrowserReload
{
    public class BrowserReloadOptions
    {

        public List<BrowserReloadFileProviderOptions> FileProviderOptions { get; set; }
        //public List<string> WebRootWatchPatterns { get; }
        //public List<string> ContentRootWatchPatterns { get; }

        public BrowserReloadOptions()
        {
            FileProviderOptions = new List<BrowserReloadFileProviderOptions>();

        }

        public BrowserReloadFileProviderOptions FileProvider(IFileProvider fileProvider)
        {
            var options = new BrowserReloadFileProviderOptions(fileProvider);
            FileProviderOptions.Add(options);
            //configureOptions(options);
            return options;
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

    }
}
