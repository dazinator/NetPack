using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipeline;

namespace NetPack
{
    public class NetPackApplicationBuilder : IApplicationBuilder, INetPackApplicationBuilder
    {

        private readonly IApplicationBuilder _builder;
        public NetPackApplicationBuilder(IApplicationBuilder appBuilder, IPipeLine pipeline, IFileProvider pipelineFileProvider)
        {
            _builder = appBuilder;
            Pipeline = pipeline;
            PipelineFileProvider = pipelineFileProvider;
        }

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            return _builder.Use(middleware);
        }

        public IApplicationBuilder New()
        {
            return _builder.New();
        }

        public RequestDelegate Build()
        {
            return _builder.Build();
        }

        public IServiceProvider ApplicationServices
        {
            get { return _builder.ApplicationServices; }
            set
            {
                _builder.ApplicationServices = value;
            }
        }
        public IFeatureCollection ServerFeatures => _builder.ServerFeatures;
        public IDictionary<string, object> Properties => _builder.Properties;
        public IPipeLine Pipeline { get; }

        public IFileProvider PipelineFileProvider { get; }
    }
}