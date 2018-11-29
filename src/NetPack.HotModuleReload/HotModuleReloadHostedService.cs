using Dazinator.AspNet.Extensions.FileProviders;
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
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace NetPack.HotModuleReload
{

    public class HotModuleReloadHostedService : IHostedService
    {
        private readonly IHostingEnvironment _env;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<HotModuleReloadHostedService> _logger;
        private readonly IOptions<HotModuleReloadOptions> _options;
        private IDisposable _changeCallbackDisposable;

        public HotModuleReloadHostedService(IHostingEnvironment env, IServiceScopeFactory serviceScopeFactory, ILogger<HotModuleReloadHostedService> logger, IOptions<HotModuleReloadOptions> options)
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

                var timestamps = watchedFiles.Select(a => new { Path = $"{a.Item1}/{a.Item2.Name}", Modified = a.Item2.LastModified }).ToArray();

                var state = new { IncludePattern = item, ExcludePatterns = excludePatterns, FileStamps = timestamps };

                allDisposables.Add(ChangeTokenHelper.OnChangeDelayed(() =>
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

                    var modified = from n in newFiles
                                   join w in state.FileStamps
                                   on $"{n.Item1}/{n.Item2.Name}" equals w.Path
                                                                        into ps
                                   from p in ps.DefaultIfEmpty()
                                   where p == null || n.Item2.LastModified > p.Modified
                                   select $"{n.Item1}/{n.Item2.Name}";


                    var modifiedFilePaths = modified.ToArray();
                    // return modifiedFiles;


                    //GetChangedFiles(state.FileStamps, newFiles);

                    //  string[] filePaths = modified.Select(f => $"{f.Item1}/{f.Item2.Name}").ToArray();
                    // notify hub
                    using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                    {
                        IHubContext<HotModuleReloadHub, IHotModuleReloadClient> hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<HotModuleReloadHub, IHotModuleReloadClient>>();
                        hubContext.Clients.All.FilesChanged(modifiedFilePaths);
                    }
                }, state, _options.Value.Delay));

            }
            // var composite = new CompositeChangeToken(allChangeTokens);
            // return composite;

            CompositeDisposable compositeDisposable = new CompositeDisposable(allDisposables);
            _changeCallbackDisposable = compositeDisposable;

            //_changeCallbackDisposable = ChangeTokenHelper.OnChangeDelayed(() =>
            //{
            //    List<IChangeToken> allChangeTokens = new List<IChangeToken>();
            //    foreach (string item in watchPatterns.IncludePatterns)
            //    {
            //        allChangeTokens.Add(fileProvider.Watch(item));
            //    }
            //    CompositeChangeToken composite = new CompositeChangeToken(allChangeTokens);
            //    return composite;

            //}, (s) =>
            //{
            //    _logger.LogInformation("detected watch pattern has changed: " + s);

            //    // now need to work out exactly which files have changed.


            //    // Then filter out if matches exclude pattern.


            //    using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            //    {
            //        IHubContext<HotModuleReloadHub, IHotModuleReloadClient> hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<HotModuleReloadHub, IHotModuleReloadClient>>();
            //        hubContext.Clients.All.Reload();
            //    }
            //}, state, _options.Value.Delay);
        }

        //private IEnumerable<Tuple<string, IFileInfo>> GetChangedFiles(IEnumerable<Tuple<string, IFileInfo>> oldFiles, IEnumerable<Tuple<string, IFileInfo>> newFiles)
        //{


        //}

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _changeCallbackDisposable?.Dispose();
            return Task.CompletedTask;
        }
    }
}
