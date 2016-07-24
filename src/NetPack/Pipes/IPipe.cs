using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipeline;

namespace NetPack.Pipes
{
    public interface IPipe
    {
        Task ProcessAsync(IPipelineContext context);
    }

    //public interface IPipeOutput
    //{
    //    /// <summary>
    //    /// The files that should be passed on to the next stage of the pipeline, as output from this pipe.
    //    /// </summary>
       
    //}
}