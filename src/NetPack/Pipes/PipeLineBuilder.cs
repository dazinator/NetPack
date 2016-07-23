using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;

namespace NetPack.Pipes
{
    public class PipeLineBuilder
    {
        public PipeLineBuilder(IApplicationBuilder appBuilder)
        {
            ApplicationBuilder = appBuilder;
        }

        public IApplicationBuilder ApplicationBuilder { get; set; }

        public PipeLineBuilder AddPipe(IPipe typescriptCompileStep)
        {
            throw new NotImplementedException();

        }

        public object Pipeline()
        {
            throw new System.NotImplementedException();
        }
    }
}