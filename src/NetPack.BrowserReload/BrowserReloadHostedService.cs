using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetPack.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.BrowserReload
{

    public class BrowserReloadHostedService : IHostedService
    {
      //  private readonly IHostingEnvironment _env;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BrowserReloadHostedService> _logger;
        private readonly IOptions<BrowserReloadOptions> _options;
        private IDisposable _changeCallbackDisposable;

        public BrowserReloadHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<BrowserReloadHostedService> logger, IOptions<BrowserReloadOptions> options)
        {
            //_env = env;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _options = options;
        }

        public BrowserReloadHostedService()
        {
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // "/netpack/built.js";
            foreach (var fileProviderOptions in _options.Value.FileProviderOptions)
            {
                WatchFiles(fileProviderOptions.FileProvider, fileProviderOptions.GetWatchPatterns());
            }           

            return Task.CompletedTask;
        }

        private void WatchFiles(IFileProvider fileProvider, IEnumerable<string> enumerable)
        {
            if (fileProvider == null || enumerable == null)
            {
                return;
            }

            foreach (string item in enumerable)
            {
                Watch(item, fileProvider);
            }
        }

        private void Watch(string pattern, IFileProvider fileProvider)
        {
            _changeCallbackDisposable = ChangeTokenHelper.OnChangeDebounce(() => fileProvider.Watch(pattern), (s) =>
            {
                _logger.LogInformation("triggering browser reload for pattern " + s);
                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    IHubContext<BrowserReloadHub, IBrowserReloadClient> hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<BrowserReloadHub, IBrowserReloadClient>>();
                    hubContext.Clients.All.Reload();
                }
            }, pattern, _options.Value.Delay);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _changeCallbackDisposable?.Dispose();
            return Task.CompletedTask;
        }
    }
}
