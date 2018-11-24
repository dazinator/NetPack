using System.Threading.Tasks;

namespace NetPack.BrowserReload
{
    public interface IBrowserReloadClient
    {
        Task Reload();
    }
}
