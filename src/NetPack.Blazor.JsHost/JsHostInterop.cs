using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using NetPack.Blazor.JsHost.HostCapabilities;

namespace NetPack.Blazor.JsHost
{
    /// <summary>
    /// Provides JavaScript interop for host capabilities.
    /// This allows JavaScript to call back into the Blazor host.
    /// </summary>
    public class JsHostInterop : IDisposable
    {
        private readonly IHostCapabilities _hostCapabilities;
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<JsHostInterop> _objectReference;

        public JsHostInterop(IHostCapabilities hostCapabilities, IJSRuntime jsRuntime)
        {
            _hostCapabilities = hostCapabilities ?? throw new ArgumentNullException(nameof(hostCapabilities));
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        /// <summary>
        /// Initializes the host API in JavaScript.
        /// </summary>
        public async Task InitializeAsync(string elementId)
        {
            _objectReference = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync("NetPackJsHost.initialize", elementId, _objectReference);
        }

        [JSInvokable]
        public async Task<bool> CheckPermission(string permission)
        {
            return await _hostCapabilities.CheckPermissionAsync(permission);
        }

        [JSInvokable]
        public async Task<UserInfo> GetUserInfo()
        {
            return await _hostCapabilities.GetUserInfoAsync();
        }

        [JSInvokable]
        public async Task<string> GetConfig(string key)
        {
            return await _hostCapabilities.GetConfigAsync(key);
        }

        [JSInvokable]
        public async Task RaiseHostEvent(string eventName, object eventData)
        {
            await _hostCapabilities.RaiseHostEventAsync(eventName, eventData);
        }

        [JSInvokable]
        public async Task<HttpResponse> Fetch(HttpRequest request)
        {
            return await _hostCapabilities.FetchAsync(request);
        }

        public void Dispose()
        {
            _objectReference?.Dispose();
        }
    }
}
