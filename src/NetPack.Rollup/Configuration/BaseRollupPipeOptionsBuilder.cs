using NetPack.Pipeline;
using NetPack.Rollup;
using System;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public class BaseRollupPipeOptionsBuilder<TInputOptions, TBuilder>
        where TInputOptions : BaseRollupInputOptions, new()
        where TBuilder: BaseRollupPipeOptionsBuilder<TInputOptions, TBuilder>
    {
        private readonly IPipelineBuilder _builder;
        private readonly TInputOptions _inputOptions;
        private readonly RollupOutputOptions _outputOptions;


        public BaseRollupPipeOptionsBuilder(IPipelineBuilder builder) : this(builder, new TInputOptions())
        {
        }

        public BaseRollupPipeOptionsBuilder(IPipelineBuilder builder, TInputOptions inputOptions) : this(builder, new TInputOptions(), new RollupOutputOptions())
        {
        }

        public BaseRollupPipeOptionsBuilder(IPipelineBuilder builder, TInputOptions inputOptions, RollupOutputOptions outputOptions)
        {
            _builder = builder;
            _inputOptions = inputOptions;
            _outputOptions = outputOptions;
        }

        public TBuilder AddPlugin(Action<IRollupPluginOptionsBuilder> configurePlugin)
        {
            RollupPluginOptionsBuilder builder = new RollupPluginOptionsBuilder();
            configurePlugin(builder);
            IPipelineBuilder.IncludeRequirement(builder.ModuleRequirement);
            InputOptions.AddPlugin(builder.ModuleRequirement.PackageName, builder.Options, builder.DefaultExportName1, builder.IsImportOnly);
            return (TBuilder)this;
        }

        public IPipelineBuilder IPipelineBuilder => _builder;

        public TInputOptions InputOptions => _inputOptions;

        public RollupOutputOptions OutputOptions => _outputOptions;

    }
}