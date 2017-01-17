using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;

namespace NetPack
{
    public class NetPackNodeServices : INetPackNodeServices
    {
        private readonly INodeServices _nodeServices;

        public NetPackNodeServices(INodeServices nodeServices)
        {
            _nodeServices = nodeServices;
        }

        public void Dispose()
        {
            _nodeServices.Dispose();
        }

        [Obsolete]
        public Task<T> Invoke<T>(string moduleName, params object[] args)
        {
            return _nodeServices.Invoke<T>(moduleName, args);
        }

        public Task<T> InvokeAsync<T>(string moduleName, params object[] args)
        {
            return _nodeServices.InvokeAsync<T>(moduleName, args);
        }

        public Task<T> InvokeAsync<T>(CancellationToken cancellationToken, string moduleName, params object[] args)
        {
            return _nodeServices.InvokeAsync<T>(cancellationToken, moduleName, args);
        }

        [Obsolete]
        public Task<T> InvokeExport<T>(string moduleName, string exportedFunctionName, params object[] args)
        {
            return _nodeServices.InvokeExport<T>(moduleName, exportedFunctionName, args);
        }

        public Task<T> InvokeExportAsync<T>(string moduleName, string exportedFunctionName, params object[] args)
        {
            return _nodeServices.InvokeExportAsync<T>(moduleName, exportedFunctionName, args);
        }

        public Task<T> InvokeExportAsync<T>(CancellationToken cancellationToken, string moduleName, string exportedFunctionName, params object[] args)
        {
            return _nodeServices.InvokeExportAsync<T>(cancellationToken, moduleName, exportedFunctionName, args);
        }
    }
}