using System.Collections.Generic;

namespace NetPack.Blazor.JsHost.HostCapabilities
{
    /// <summary>
    /// Represents information about the current user.
    /// </summary>
    public class UserInfo
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsAuthenticated { get; set; }
        public Dictionary<string, string> Claims { get; set; }

        public UserInfo()
        {
            Claims = new Dictionary<string, string>();
        }
    }
}
