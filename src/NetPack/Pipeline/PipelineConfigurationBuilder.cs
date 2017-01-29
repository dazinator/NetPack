using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NetPack.RequireJs;
using NetPack.Requirements;
using Dazinator.AspNet.Extensions.FileProviders.Directory;

namespace NetPack.Pipeline
{

    public class PipelineConfigurationBuilder : IPipelineConfigurationBuilder, IPipelineBuilder
    {

        public PipelineConfigurationBuilder(IServiceProvider serviceProvider, IDirectory sourcesOutputDirectory)
        {
            ServiceProvider = serviceProvider;
            Pipes = new List<PipeContext>();
            Requirements = new List<IRequirement>();
            SourcesOutputDirectory = sourcesOutputDirectory;
            Name = Guid.NewGuid().ToString();
            //FileProvider = fileProvider ?? GetHostingEnvironmentContentFileProvider();
        }

        #region  IPipelineConfigurationBuilder

        public IServiceProvider ServiceProvider { get; set; }

        public IPipelineBuilder WithHostingEnvironmentWebrootProvider()
        {
            FileProvider = GetHostingEnvironmentWebRootFileProvider();
            return this;
        }

        public IPipelineBuilder WithHostingEnvironmentContentProvider()
        {
            FileProvider = GetHostingEnvironmentContentFileProvider();
            return this;
        }

        public IPipelineBuilder WithFileProvider(IFileProvider fileProvider)
        {
            if (fileProvider == null)
            {
                throw new ArgumentNullException(nameof(fileProvider));
            }
            FileProvider = fileProvider;
            return this;
        }

        #endregion

        public IFileProvider FileProvider { get; set; }

        public string BaseRequestPath { get; set; }

        public string Name { get; set; }

        public IDirectory SourcesOutputDirectory { get; }

        public List<IRequirement> Requirements { get; set; }

        public List<PipeContext> Pipes { get; set; }

        public IPipelineBuilder AddPipe(Action<PipelineInputBuilder> inputBuilder, IPipe pipe)
        {
            var builder = new PipelineInputBuilder();
            inputBuilder(builder);
            Pipes.Add(new PipeContext(builder.Input, pipe));
            return this;
        }

        public IFileProvider GetHostingEnvironmentContentFileProvider()
        {
            var hostingEnv = ServiceProvider.GetRequiredService<IHostingEnvironment>();
            var fileProvider = hostingEnv.ContentRootFileProvider;
            if (fileProvider == null)
            {
                throw new InvalidOperationException("The IHostingEnvironment doesn't have a ContentRootFileProvider initialised.");
            }
            return fileProvider;
        }

        public IFileProvider GetHostingEnvironmentWebRootFileProvider()
        {
            var hostingEnv = ServiceProvider.GetRequiredService<IHostingEnvironment>();
            var fileProvider = hostingEnv.WebRootFileProvider;
            if (fileProvider == null)
            {
                throw new InvalidOperationException("The IHostingEnvironment doesn't have a WebRootFileProvider initialised.");
            }
            return fileProvider;
        }

        public bool WachInput { get; protected set; }

        public IPipelineBuilder Watch()
        {
            this.WachInput = true;
            return this;
        }

        public IPipeLine BuildPipeLine()
        {
            var pipeLine = new Pipeline(FileProvider, Pipes, Requirements, SourcesOutputDirectory, BaseRequestPath);
            // var fileProvider = new NetPackPipelineFileProvider(pipeLine);
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

        public IPipelineBuilder UseBaseRequestPath(string baseRequestPath = null)
        {

            if (baseRequestPath != null)
            {
                if (!baseRequestPath.StartsWith("/"))
                {
                    baseRequestPath = "/" + baseRequestPath;
                }
            }

            BaseRequestPath = baseRequestPath;
            return this;
        }
    }



}