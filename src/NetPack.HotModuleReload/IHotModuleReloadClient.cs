using System.Threading.Tasks;

namespace NetPack.HotModuleReload
{
    public interface IHotModuleReloadClient
    {
        Task FilesChanged(string[] files);
    }
}
