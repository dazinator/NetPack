using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.AspNetCore.Hosting;

namespace NetPack
{
    public class NetPackNodeServices : INetPackNodeServices
    {
        private readonly INodeServices _nodeServices;

#if NODESERVICESASYNC
        private readonly IApplicationLifetime _lifetime;

        public NetPackNodeServices(INodeServices nodeServices, IApplicationLifetime lifetime)
        {
            _nodeServices = nodeServices;
           _lifetime = lifetime;
        }
#else
        public NetPackNodeServices(INodeServices nodeServices)
        {
            _nodeServices = nodeServices;
        }
#endif


        public void Dispose()
        {
            _nodeServices.Dispose();
        }

        [Obsolete]
        public Task<T> Invoke<T>(string moduleName, params object[] args)
        {
#if NODESERVICESASYNC
             return _nodeServices.InvokeAsync<T>(moduleName, args);
#else
            return _nodeServices.Invoke<T>(moduleName, args);
#endif

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
#if NODESERVICESASYNC
             return _nodeServices.InvokeExportAsync<T>(moduleName, exportedFunctionName, args);
#else
            return _nodeServices.InvokeExport<T>(moduleName, exportedFunctionName, args);
#endif
        }

        public Task<T> InvokeExportAsync<T>(string moduleName, string exportedFunctionName, params object[] args)
        {
            return _nodeServices.InvokeExportAsync<T>(moduleName, exportedFunctionName, args);
        }

        public Task<T> InvokeExportAsync<T>(CancellationToken cancellationToken, string moduleName, string exportedFunctionName, params object[] args)
        {
            return _nodeServices.InvokeExportAsync<T>(cancellationToken, moduleName, exportedFunctionName, args);
        }
        public StringAsTempFile CreateStringAsTempFile(string content)
        {
#if NODESERVICESASYNC
            return new StringAsTempFile(content, _lifetime.ApplicationStopping);
#else
            return new StringAsTempFile(content);
#endif
        }
    }
}