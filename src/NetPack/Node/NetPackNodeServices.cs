using System;
using System.Threading;
using System.Threading.Tasks;
using Jering.Javascript.NodeJS;

namespace NetPack
{
    public class NetPackNodeServices : INetPackNodeServices
    {
        private readonly INodeJSService _nodeServices;

        public string ProjectDir { get; set; } 


        //  private readonly IApplicationLifetime _lifetime;

        public NetPackNodeServices(INodeJSService nodeServices)
        {
            ProjectDir=   Environment.CurrentDirectory;
            _nodeServices = nodeServices;
                // ProjectDir = projectDir;
            // _lifetime = lifetime;
        }


        public void Dispose()
        {
            _nodeServices.Dispose();
        }

//         [Obsolete]
//         public Task<T> Invoke<T>(string moduleName, params object[] args)
//         {
// #if NODESERVICESASYNC
//              return _nodeServices.InvokeAsync<T>(moduleName, args);
// #else
//             return _nodeServices.Invoke<T>(moduleName, args);
// #endif
//
//         }

        // public Task<T> InvokeAsync<T>(string moduleName, params object[] args)
        // {
        //     return _nodeServices.InvokeAsync<T>(moduleName, args);
        // }

        // public Task<T> InvokeAsync<T>(CancellationToken cancellationToken, string moduleName, params object[] args)
        // {
        //     return _nodeServices.InvokeAsync<T>(cancellationToken, moduleName, args);
        // }

        public async Task<T> InvokeExportAsync<T>(StringAsTempFile script, string exportedFunctionName, object[] args = null,
            CancellationToken cancellationToken = default)
        {
            var factory = script.contentFactory;
            var scriptContent = factory(); // Call the factory to get the actual script string

            var result = await _nodeServices.InvokeFromStringAsync<T>(scriptContent, script.cacheIdentifier,
                exportedFunctionName, args: args, cancellationToken);

            return result;
        }

        public Task<TResult> InvokeExportAsync<TRequest, TResult>(StringAsTempFile script, string exportedFunctionName, TRequest request,
            CancellationToken cancellationToken = default)
        {
            var args = new object[] { request };
            return InvokeExportAsync<TResult>(script, exportedFunctionName, args, cancellationToken);
        }

        // public Task<T> InvokeExportAsync<T>(CancellationToken cancellationToken, string moduleName, string exportedFunctionName, params object[] args)
        // {
        //     return _nodeServices.InvokeExportAsync<T>(cancellationToken, moduleName, exportedFunctionName, args);
        // }
        // public StringAsTempFile CreateStringAsTempFile(string content, string cacheIdentifier)
        // {
        //     return new StringAsTempFile(cacheIdentifier);
        // }
    }

    public record StringAsTempFile(string cacheIdentifier, Func<string> contentFactory);
}