using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NetPack.Pipeline;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NetPack
{
    public class RequestHaltingMiddlewareOptions
    {
        public static TimeSpan DefaultTimeout = new TimeSpan(0, 1, 0);

        public RequestHaltingMiddlewareOptions()
        {
            Timeout = DefaultTimeout;
        }

        public TimeSpan Timeout { get; set; }

    }

    public class RequestHaltingMiddleware
    {
        private readonly RequestDelegate _next;
        // private readonly ILogger _logger;
        private readonly PipelineManager _pipelineManager;

        private readonly RequestHaltingMiddlewareOptions _options;



        public RequestHaltingMiddleware(RequestDelegate next, RequestHaltingMiddlewareOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context, ILogger<RequestHaltingMiddleware> logger, PipelineManager pipelineManager)
        {
            // When a request is received, 
            logger.LogDebug("Handling request: " + context.Request.Path);
            // if the incoming request is for a file, make sure its not a file that is currently locked.
            // We do this by waiting on any existing lock with the path name if it exists.
            if (IsValidMethod(context.Request) && !PathEndsInSlash(context.Request.Path))
            {
                bool wait = true;
                while(wait)
                {
                    wait = pipelineManager.PipeLines.Any(a =>
                    {
                        if (!a.Value.IsBusy)
                        {
                            return false;
                        }
                        var file = a.Value.WebrootFileProvider.GetFileInfo(context.Request.Path);
                        return file.Exists && !file.IsDirectory && a.Value.IsBusy;
                    });

                    if(wait)
                    {
                        logger.LogDebug("Waiting on busy pipeline for file: " + context.Request.Path);
                        await Task.Delay(500);
                    }
                }               

               // await FileLocks.WaitIfLockedAsync(context.Request.Path, _options.Timeout, context.RequestAborted);
                //await FileRequestServices.WhenFileNotLocked(context.Request.Path, _options.Timeout, context.RequestAborted);
               // logger.LogDebug("File processing finished." + context.Request.Path);
            }

            await _next.Invoke(context);


        }

        internal static bool PathEndsInSlash(PathString path)
        {
            return path.Value.EndsWith("/", StringComparison.Ordinal);
        }

        public bool IsValidMethod(HttpRequest request)
        {
            return IsGetOrHeadMethod(request.Method);
        }

        internal static bool IsGetOrHeadMethod(string method)
        {

            return IsGetMethod(method) || IsHeadMethod(method);
        }

        internal static bool IsGetMethod(string method)
        {
            return string.Equals("GET", method, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsHeadMethod(string method)
        {
            return string.Equals("HEAD", method, StringComparison.OrdinalIgnoreCase);
        }



    }
}