using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace NetPack.Web.WIP
{
    public interface IBrowserReloadClient
    {
        Task Reload(string message);
    }

    public class BrowserReloadHub : Hub<IBrowserReloadClient>
    {
        public async Task TriggerReload(string message)
        {
            await Clients.All.Reload(message);
        }
    }

    public class BrowserReloadOptions
    {

    }

    public class BrowserReloadHostedService : IHostedService
    {
        private readonly IHostingEnvironment _env;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BrowserReloadHostedService> _logger;
        private IDisposable _changeCallbackDisposable;

        public BrowserReloadHostedService(IHostingEnvironment env, IServiceScopeFactory serviceScopeFactory, ILogger<BrowserReloadHostedService> logger)
        {
            _env = env;
            _serviceScopeFactory = serviceScopeFactory;
            this._logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            string pattern = "**/*";
            Watch(pattern);
            return Task.CompletedTask;
        }

        private void Watch(string pattern)
        {
            _changeCallbackDisposable = ChangeToken.OnChange(() => _env.WebRootFileProvider.Watch(pattern), (s) =>
             {
                 _logger.LogInformation("triggering browser reload");
                 using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                 {
                     IHubContext<BrowserReloadHub, IBrowserReloadClient> hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<BrowserReloadHub, IBrowserReloadClient>>();
                     hubContext.Clients.All.Reload("change: " + pattern);
                  }

             }, pattern);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _changeCallbackDisposable?.Dispose();
            return Task.CompletedTask;
        }
    }
}
