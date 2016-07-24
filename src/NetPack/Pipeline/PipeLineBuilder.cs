using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipes;

namespace NetPack.Pipeline
{
    public class PipeLineBuilder : IPipelineBuilder, IPipelineInputBuilder
    {
        public PipeLineBuilder(IApplicationBuilder appBuilder)
        {
            ApplicationBuilder = appBuilder;
            Pipes = new List<IPipe>();
        }

        public IApplicationBuilder ApplicationBuilder { get; set; }

        //public IFileProvider FileProvider { get; set; }

        public List<IPipe> Pipes { get; set; }

        public PipeLineBuilder AddPipe(IPipe pipe)
        {
            Pipes.Add(pipe);
            return this;
        }

        public IPipelineInputBuilder WithInput(PipelineInput sources)
        {
            Sources = sources;
            return this;
        }

        public IPipelineInputBuilder WithInput(Func<PipelineInputBuilder, PipelineInput> sourcesBuilder, IFileProvider fileProvider = null)
        {
            var sourcesFileProvider = fileProvider ?? GetDefaultFileProvider();
            var builder = new PipelineInputBuilder(sourcesFileProvider);
            var sources = sourcesBuilder(builder);
            Sources = sources;
            return this;
        }

        public IFileProvider GetDefaultFileProvider()
        {
            var hostingEnv = ApplicationBuilder.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var fileProvider = hostingEnv.ContentRootFileProvider;
            if (fileProvider == null)
            {
                throw new InvalidOperationException("The IHostingEnvironment doesn't have a ContentRootFileProvider initialised.");
            }
            return fileProvider;
        }

        public PipelineInput Sources { get; protected set; }

        public bool WachInput { get; protected set; }

        public IPipelineInputBuilder WatchInputForChanges()
        {
            this.WachInput = true;
            return this;
        }

        public IPipeLine BuildPipeLine()
        {
            var pipeLine = new Pipeline(Sources, Pipes, this.WachInput);
            return pipeLine;

        }
    }



}