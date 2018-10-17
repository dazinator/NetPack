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
        private readonly RollupInputOptions _inputOptions;
        private readonly RollupOutputOptions _outputOptions;

        public RollupPipeOptionsBuilder(IPipelineBuilder builder): this(builder, new RollupInputOptions())
        {           
        }

        public RollupPipeOptionsBuilder(IPipelineBuilder builder, RollupInputOptions inputOptions): this(builder, new RollupInputOptions(), new RollupOutputOptions())
        {           
        }

        public RollupPipeOptionsBuilder(IPipelineBuilder builder, RollupInputOptions inputOptions, RollupOutputOptions outputOptions)
        {
            _builder = builder;
            _inputOptions = inputOptions;
            _outputOptions = outputOptions;
        }

        public RollupPipeOptionsBuilder AddPlugin(Action<IRollupPluginOptionsBuilder> configurePlugin)
        {
            RollupPluginOptionsBuilder builder = new RollupPluginOptionsBuilder(this);
            configurePlugin(builder);
            builder.Build();
            return this;
        }


        public IPipelineBuilder IPipelineBuilder => _builder;

        public RollupInputOptions InputOptions => _inputOptions;

        public RollupOutputOptions OutputOptions => _outputOptions;

    }
}