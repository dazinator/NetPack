using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipes;

namespace NetPack.Pipeline
{
    public interface IPipelineBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; set; }

        PipeLineBuilder AddPipe(IPipe pipe);

        IPipelineInputBuilder WithInput(PipelineInput sources);

        IPipelineInputBuilder WithInput(Func<PipelineInputBuilder, PipelineInput> sourcesBuilder, IFileProvider fileProvider = null);


    }
}