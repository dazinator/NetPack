using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace NetPack.Web.WIP
{
    public class BrowserReloadHub : Hub
    {
        public async Task TriggerReload(string message)
        {
            await Clients.All.SendAsync("Reload", message);
        }
    }
}
