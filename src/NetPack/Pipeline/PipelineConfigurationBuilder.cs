﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using NetPack.Pipes;
using NetPack.Requirements;
using Dazinator.AspNet.Extensions.FileProviders.Directory;

namespace NetPack.Pipeline
{

    public class PipeConfiguration
    {

        public IPipe Pipe { get; set; }
        public PipelineInput Input { get; set; }
        public DateTime LastProcessedEndTime { get; set; } = DateTime.MinValue.ToUniversalTime();
        public DateTime LastProcessStartTime { get; set; } = DateTime.MinValue.ToUniversalTime();
        public bool IsProcessing { get; set; }

        /// <summary>
        /// Returns true if the pipe has updated inputs that need to be processed.
        /// </summary>
        /// <returns></returns>
        internal bool IsDirty()
        {
            var isDirty = (Input.LastChanged > LastProcessStartTime);
            isDirty = isDirty && Input.LastChanged <= LastProcessedEndTime;
            isDirty = isDirty || Input.LastChanged > LastProcessedEndTime;

            return isDirty;
        }
    }

    public class PipelineConfigurationBuilder : IPipelineConfigurationBuilder, IPipelineBuilder
    {

        public PipelineConfigurationBuilder(IApplicationBuilder appBuilder, IDirectory sourcesOutputDirectory)
        {
            ApplicationBuilder = appBuilder;
            Pipes = new List<PipeConfiguration>();
            Requirements = new List<IRequirement>();
            SourcesOutputDirectory = sourcesOutputDirectory;
            //FileProvider = fileProvider ?? GetHostingEnvironmentContentFileProvider();
        }

        #region  IPipelineConfigurationBuilder

        public IApplicationBuilder ApplicationBuilder { get; set; }

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

        public IDirectory SourcesOutputDirectory { get; }


        // public IDirectory Directory { get; set; }

        //public IFileProvider FileProvider { get; set; }

        public List<IRequirement> Requirements { get; set; }

        public List<PipeConfiguration> Pipes { get; set; }

        public IPipelineBuilder AddPipe(Action<PipelineInputBuilder> inputBuilder, IPipe pipe)
        {
            var builder = new PipelineInputBuilder();
            inputBuilder(builder);
            Pipes.Add(new PipeConfiguration() { Input = builder.Input, Pipe = pipe });
            return this;
        }

        public IFileProvider GetHostingEnvironmentContentFileProvider()
        {
            var hostingEnv = ApplicationBuilder.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var fileProvider = hostingEnv.ContentRootFileProvider;
            if (fileProvider == null)
            {
                throw new InvalidOperationException("The IHostingEnvironment doesn't have a ContentRootFileProvider initialised.");
            }
            return fileProvider;
        }

        public IFileProvider GetHostingEnvironmentWebRootFileProvider()
        {
            var hostingEnv = ApplicationBuilder.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var fileProvider = hostingEnv.WebRootFileProvider;
            if (fileProvider == null)
            {
                throw new InvalidOperationException("The IHostingEnvironment doesn't have a WebRootFileProvider initialised.");
            }
            return fileProvider;
        }

        public bool WachInput { get; protected set; }

        // public string WebRootPath { get; set; }

        public IPipelineBuilder Watch()
        {
            this.WachInput = true;
            return this;
        }

        //public IPipelineInputOptionsBuilder FromWebRoot(string webrootPath)
        //{
        //    WebRootPath = webrootPath;
        //    return this;
        //}

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