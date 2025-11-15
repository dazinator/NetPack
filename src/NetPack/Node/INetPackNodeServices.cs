using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack
{
    public interface INetPackNodeServices
    {
        string ProjectDir { get; set; } 
     //   StringAsTempFile CreateStringAsTempFile(string content);
     Task<T> InvokeExportAsync<T>(StringAsTempFile script, string exportedFunctionName, object[] args = null,
         CancellationToken cancellationToken = default);
     
     
     Task<TResult> InvokeExportAsync<TRequest, TResult>(StringAsTempFile script, string exportedFunctionName, TRequest request,
         CancellationToken cancellationToken = default);

    }
}