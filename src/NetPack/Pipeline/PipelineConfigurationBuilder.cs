using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipes;
using NetPack.Requirements;

namespace NetPack.Pipeline
{
    public class PipelineConfigurationBuilder : IPipelineConfigurationBuilder, IPipelineInputOptionsBuilder, IPipelineBuilder
    {
        
        public PipelineConfigurationBuilder(IApplicationBuilder appBuilder)
        {
            ApplicationBuilder = appBuilder;
            Pipes = new List<IPipe>();
            Requirements = new List<IRequirement>();
        }

        public IApplicationBuilder ApplicationBuilder { get; set; }


        //public IFileProvider FileProvider { get; set; }

        public List<IRequirement> Requirements { get; set; }

        public List<IPipe> Pipes { get; set; }

        public IPipelineBuilder AddPipe(IPipe pipe)
        {
            Pipes.Add(pipe);
            return this;
        }

        public IPipelineInputOptionsBuilder Take(PipelineInput sources)
        {
            Sources = sources;
            return this;
        }

        public IPipelineInputOptionsBuilder Take(Action<PipelineInputBuilder> sourcesBuilder, IFileProvider fileProvider = null)
        {
            var sourcesFileProvider = fileProvider ?? GetDefaultFileProvider();
            var builder = new PipelineInputBuilder(sourcesFileProvider);
            sourcesBuilder(builder);
            Sources = builder.Input;
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

        public IPipelineInputOptionsBuilder Watch()
        {
            this.WachInput = true;
            return this;
        }

        public IPipelineBuilder BeginPipeline()
        {
            return this;
        }

        IPipelineBuilder IPipelineBuilder.AddPipe(IPipe pipe)
        {
            return AddPipe(pipe);
        }

        public IPipeLine BuildPipeLine()
        {
            var pipeLine = new Pipeline(Sources, Pipes, Requirements);
            return pipeLine;

        }

        public IPipelineBuilder IncludeRequirement(IRequirement requirement)
        {
            if (!Requirements.Contains(requirement))
            {
                Requirements.Add(requirement);
            }
            return this;
        }
    }



}