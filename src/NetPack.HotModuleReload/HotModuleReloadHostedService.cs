using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NetPack.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;

namespace NetPack.HotModuleReload
{

    public class HotModuleReloadHostedService : IHostedService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<HotModuleReloadHostedService> _logger;
        private readonly IOptions<HotModuleReloadOptions> _options;
        private IDisposable _changeCallbackDisposable;

        public HotModuleReloadHostedService(IWebHostEnvironment env, IServiceScopeFactory serviceScopeFactory, ILogger<HotModuleReloadHostedService> logger, IOptions<HotModuleReloadOptions> options)
        {
            _env = env;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _options = options;
        }

        public HotModuleReloadHostedService()
        {
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // "/netpack/built.js";
            Watch(_env.WebRootFileProvider, _options.Value.WebRootWatchPatterns);
            Watch(_env.ContentRootFileProvider, _options.Value.ContentRootWatchPatterns);

            return Task.CompletedTask;
        }


        private void Watch(IFileProvider fileProvider, WatchPatterns watchPatterns)
        {
            if (fileProvider == null || watchPatterns == null)
            {
                return;
            }

            List<IChangeToken> allChangeTokens = new List<IChangeToken>();
            List<IDisposable> allDisposables = new List<IDisposable>();

            foreach (string item in watchPatterns.IncludePatterns)
            {

                string[] excludePatterns = watchPatterns.ExcludePatterns.ToArray();
                IEnumerable<Tuple<string, IFileInfo>> watchedFiles = fileProvider.Search(new string[1] { item }, excludePatterns);
                var timestamps = watchedFiles.Select(a => new { Path = $"{a.Item1}/{a.Item2.Name}", Modified = a.Item2.LastModified }).ToDictionary(a=>a.Path, b=>b.Modified);
                var state = new { IncludePattern = item, ExcludePatterns = excludePatterns, FileStamps = timestamps };

                allDisposables.Add(ChangeTokenHelper.OnChangeDebounce(() =>
                {
                    return fileProvider.Watch(item);

                    //List<IChangeToken> allChangeTokens = new List<IChangeToken>();
                    //foreach (string item in watchPatterns.IncludePatterns)
                    //{
                    //    allChangeTokens.Add(fileProvider.Watch(item));
                    //}
                    //var composite = new CompositeChangeToken(allChangeTokens);
                    //return composite;

                }, (s) =>
                {

                    _logger.LogInformation("detected watch pattern has changed: " + state.IncludePattern);

                    // now need to work out exactly which files have changed.
                    IEnumerable<Tuple<string, IFileInfo>> newFiles = fileProvider.Search(new string[1] { state.IncludePattern }, state.ExcludePatterns);

                    var changedFiles = new List<string>();

                    foreach (var maybeNewFile in newFiles)
                    {
                        var maybeNewFilePath = $"{maybeNewFile.Item1}/{maybeNewFile.Item2.Name}";
                        if (s.FileStamps.ContainsKey(maybeNewFilePath))
                        {
                            var previousLastModified = s.FileStamps[maybeNewFilePath];
                            if(maybeNewFile.Item2.LastModified > previousLastModified)
                            {
                                // file has changed.
                                changedFiles.Add(maybeNewFilePath);
                                s.FileStamps[maybeNewFilePath] = maybeNewFile.Item2.LastModified;
                            }
                        }
                        else
                        {
                            // new file added to file system.
                            changedFiles.Add(maybeNewFilePath);
                            s.FileStamps[maybeNewFilePath] = maybeNewFile.Item2.LastModified;
                        }
                    }

                    using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                    {
                        IHubContext<HotModuleReloadHub, IHotModuleReloadClient> hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<HotModuleReloadHub, IHotModuleReloadClient>>();
                        hubContext.Clients.All.FilesChanged(changedFiles.ToArray());
                    }
                }, state, _options.Value.Delay));

            }          

            CompositeDisposable compositeDisposable = new CompositeDisposable(allDisposables);
            _changeCallbackDisposable = compositeDisposable;

        }       

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _changeCallbackDisposable?.Dispose();
            return Task.CompletedTask;
        }
    }
}
