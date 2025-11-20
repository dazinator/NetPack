using System.Threading.Tasks;

namespace NetPack.Blazor.JsHost.HostCapabilities
{
    /// <summary>
    /// Interface for checking user permissions.
    /// Applications can provide their own implementation.
    /// </summary>
    public interface IPermissionChecker
    {
        Task<bool> CheckPermissionAsync(string permission);
    }
}
