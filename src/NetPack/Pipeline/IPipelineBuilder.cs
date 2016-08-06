using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipes;

namespace NetPack.Pipeline
{
    public interface IPipelineConfigurationBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; set; }

        IPipelineInputOptionsBuilder Take(PipelineInput sources);

        IPipelineInputOptionsBuilder Take(Action<PipelineInputBuilder> sourcesBuilder, IFileProvider fileProvider = null);


    }
}