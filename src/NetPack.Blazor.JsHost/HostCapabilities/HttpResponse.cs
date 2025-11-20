using System.Collections.Generic;

namespace NetPack.Blazor.JsHost.HostCapabilities
{
    /// <summary>
    /// Represents an HTTP response returned to JavaScript from the host.
    /// </summary>
    public class HttpResponse
    {
        public int Status { get; set; }
        public string StatusText { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public bool Ok { get; set; }

        public HttpResponse()
        {
            Headers = new Dictionary<string, string>();
        }
    }
}
