using System.Collections.Generic;

namespace NetPack.Blazor.JsHost.HostCapabilities
{
    /// <summary>
    /// Represents an HTTP request from JavaScript to be executed by the host.
    /// </summary>
    public class HttpRequest
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }

        public HttpRequest()
        {
            Method = "GET";
            Headers = new Dictionary<string, string>();
        }
    }
}
