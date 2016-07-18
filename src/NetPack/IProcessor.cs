using System.Threading.Tasks;

namespace NetPack
{
    public interface IProcessor
    {
        Task<string> ProcessInputAsync(FileProcessContext fileProcessContext);
    }
}