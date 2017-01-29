using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NetPack.Pipeline;

namespace NetPack
{
    public class RequestHaltingMiddleware
    {
        private readonly RequestDelegate _next;
        // private readonly ILogger _logger;
        private readonly PipelineManager _pipelineManager;

        public RequestHaltingMiddleware(RequestDelegate next, PipelineManager pipelineManager)
        {
            _next = next;
            _pipelineManager = pipelineManager;

        }

        public async Task Invoke(HttpContext context)
        {
            // When a request is received, 
            //_logger.LogInformation("Handling request: " + context.Request.Path);
            await _next.Invoke(context);
            //_logger.LogInformation("Finished handling request.");
        }
    }
}