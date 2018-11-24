using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace NetPack.BrowserReload
{
    public class BrowserReloadHub : Hub<IBrowserReloadClient>
    {
        public async Task TriggerReload()
        {
            await Clients.All.Reload();
        }
    }
}
