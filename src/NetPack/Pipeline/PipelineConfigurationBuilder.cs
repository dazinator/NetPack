using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NetPack.Requirements;
using Dazinator.Extensions.FileProviders.InMemory.Directory;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace NetPack.Pipeline
{

    public class PipelineConfigurationBuilder : IPipelineConfigurationBuilder, IPipelineBuilder
    {

        public PipelineConfigurationBuilder(IServiceProvider serviceProvider, IDirectory sourcesOutputDirectory)
        {
            ServiceProvider = serviceProvider;
            Pipes = new List<PipeProcessor>();
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

        public PathString BaseRequestPath { get; set; }

        public string Name { get; set; }

        public IDirectory SourcesOutputDirectory { get; }

        public List<IRequirement> Requirements { get; set; }

        public List<PipeProcessor> Pipes { get; set; }

        public IPipelineBuilder AddPipe(Action<PipelineInputBuilder> inputBuilder, IPipe pipe)
        {
            var builder = new PipelineInputBuilder();
            inputBuilder(builder);
            Pipes.Add(new PipeProcessor(builder.Input, pipe, ServiceProvider.GetRequiredService<ILogger<PipeProcessor>>()));
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

        public bool WatchInput { get; protected set; }

        public int WatchTriggerDelay { get; protected set; }

        /// <summary>
        /// Watch the inputs to the pipeline and re-process any pipes if inputs change.
        /// </summary>
        /// <param name="triggerDelay">Number of milliseconds to delay after initial trigger of a file change token, in case file change token is triggered multiple times in quick succession.
        /// This is necessary to workaround issue with IFileProvider.Watch() signalling multiple change tokens in quick succession when a file is changed. See https://github.com/aspnet/AspNetCore/issues/2542 for info.</param>
        /// <returns></returns>
        public IPipelineBuilder Watch(int triggerDelay = 200)
        {
            this.WatchTriggerDelay = triggerDelay;
            this.WatchInput = true;
            return this;
        }

        public IPipeLine BuildPipeLine()
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<Pipeline>>();
            var pipeLine = new Pipeline(FileProvider, Pipes, Requirements, SourcesOutputDirectory, logger, BaseRequestPath, shouldPerformRequirementsCheck: ShouldPerformRequirementsCheckPredicate);
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

        public Predicate<IPipeLine> ShouldPerformRequirementsCheckPredicate { get; set; }

        /// <summary>
        /// A predicate that returns whether requirements for the pipeline should be checked when the pipeline is first initialised. For example, the pipeline may have requirements that certain npm packages are installed, or other environmental things. If this check is disabled then it will result in quicker start up times, but you have to be sure all requirements are met.
        /// for the operation of the pipeline on the current environment. 
        /// </summary>
        /// <returns></returns>
        public IPipelineBuilder ShouldPerformRequirementsCheck(Predicate<IPipeLine> predicate)
        {
            ShouldPerformRequirementsCheckPredicate = predicate;
            return this;
        }

        public IPipelineBuilder UseBaseRequestPath(PathString baseRequestPath)
        {      
            BaseRequestPath = baseRequestPath;
            return this;
        }
    }
}