using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using NetPack.Pipes;
using NetPack.Requirements;

namespace NetPack.Pipeline
{

    public class PipeConfiguration
    {
        public IPipe Pipe { get; set; }
        public PipelineInput Input { get; set; }

    }

    public class PipelineConfigurationBuilder : IPipelineConfigurationBuilder, IPipelineInputOptionsBuilder, IPipelineBuilder
    {

        public PipelineConfigurationBuilder(IApplicationBuilder appBuilder, IFileProvider fileProvider = null)
        {
            ApplicationBuilder = appBuilder;
            Pipes = new List<PipeConfiguration>();
            Requirements = new List<IRequirement>();
            FileProvider = fileProvider ?? GetDefaultFileProvider();
        }

        public IFileProvider FileProvider { get; set; }

        public IApplicationBuilder ApplicationBuilder { get; set; }

        //public IFileProvider FileProvider { get; set; }

        public List<IRequirement> Requirements { get; set; }

        public List<PipeConfiguration> Pipes { get; set; }

        public IPipelineBuilder AddPipe(Action<PipelineInputBuilder> inputBuilder, IPipe pipe)
        {
            var builder = new PipelineInputBuilder(FileProvider);
            inputBuilder(builder);
            Pipes.Add(new PipeConfiguration() { Input = builder.Input });
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

        public bool WachInput { get; protected set; }

        public string WebRootPath { get; set; }

        public IPipelineInputOptionsBuilder Watch()
        {
            this.WachInput = true;
            return this;
        }

        public IPipelineInputOptionsBuilder FromWebRoot(string webrootPath)
        {
            WebRootPath = webrootPath;
            return this;
        }

        public IPipeLine BuildPipeLine()
        {
            var pipeLine = new Pipeline(Pipes, Requirements);
            var fileProvider = new NetPackPipelineFileProvider(pipeLine);
            //  public IFileProvider FileProvider { get; set; }
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