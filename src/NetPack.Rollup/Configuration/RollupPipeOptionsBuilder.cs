using NetPack.Pipeline;
using NetPack.Rollup;
using System;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public class RollupPipeOptionsBuilder
    {
        private readonly IPipelineBuilder _builder;
        private readonly RollupPipeOptions _options;


        public RollupPipeOptionsBuilder(IPipelineBuilder builder, RollupPipeOptions options)
        {
            _builder = builder;
            _options = options;
        }

        public RollupPipeOptionsBuilder AddPlugin(Action<IRollupPluginOptionsBuilder> configurePlugin)
        {
            RollupPluginOptionsBuilder builder = new RollupPluginOptionsBuilder(this);
            configurePlugin(builder);
            builder.Build();
            return this;
        }

        public IPipelineBuilder IPipelineBuilder => _builder;

        public RollupPipeOptions RollupPipeOptions => _options;

    }
}