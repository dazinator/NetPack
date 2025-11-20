using System.Threading.Tasks;

namespace NetPack.Blazor.JsHost.HostCapabilities
{
    /// <summary>
    /// Interface defining the capabilities that a Blazor host can expose to hosted JavaScript applications.
    /// </summary>
    public interface IHostCapabilities
    {
        /// <summary>
        /// Checks if the user has a specific permission.
        /// </summary>
        Task<bool> CheckPermissionAsync(string permission);

        /// <summary>
        /// Gets information about the current user.
        /// </summary>
        Task<UserInfo> GetUserInfoAsync();

        /// <summary>
        /// Gets a configuration value by key.
        /// </summary>
        Task<string> GetConfigAsync(string key);

        /// <summary>
        /// Raises an event that the host can handle.
        /// </summary>
        Task RaiseHostEventAsync(string eventName, object eventData);

        /// <summary>
        /// Executes an HTTP request using the host's HttpClient.
        /// </summary>
        Task<HttpResponse> FetchAsync(HttpRequest request);
    }
}
