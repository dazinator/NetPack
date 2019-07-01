using NetPack.BrowserReload;

namespace NetPack.Blazor
{
    public static class BrowserReloadOptionsExtensions
    {
        public static BrowserReloadFileProviderOptions WithBlazorClientStaticFiles<TClient>(this BrowserReloadOptions reloadOptions)
        {
            return reloadOptions.FileProvider(BlazorClientAppFileProviderHelper.GetStaticFileProvider<TClient>());
        }

        public static BrowserReloadFileProviderOptions WithBlazorClientProjectFiles<TClient>(this BrowserReloadOptions reloadOptions)
        {
            return reloadOptions.FileProvider(BlazorClientAppFileProviderHelper.GetProjectFileProvider<TClient>());
        }
    }
}
