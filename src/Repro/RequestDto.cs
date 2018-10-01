using System.Collections.Generic;

namespace Repro
{
    public class RequestDto
    {
        public RequestDto()
        {
            Items = new Dictionary<string, string>();
        }
        public Dictionary<string, string> Items { get; set; }
        public Dictionary<string, string> EchoState { get; set; }

    }
}
