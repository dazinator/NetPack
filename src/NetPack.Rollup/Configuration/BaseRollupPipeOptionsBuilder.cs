using NetPack.Pipeline;
using NetPack.Rollup;
using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public class BaseRollupPipeOptionsBuilder<TInputOptions, TOutputOptions, TBuilder>
        where TInputOptions : BaseRollupInputOptions, new()
        where TOutputOptions : BaseRollupOutputOptions, new()
        where TBuilder : BaseRollupPipeOptionsBuilder<TInputOptions, TOutputOptions, TBuilder>
    {
        private readonly IPipelineBuilder _builder;
        private readonly TInputOptions _inputOptions;
        private readonly List<TOutputOptions> _outputOptions;


        public BaseRollupPipeOptionsBuilder(IPipelineBuilder builder) : this(builder, new TInputOptions())
        {
        }

        public BaseRollupPipeOptionsBuilder(IPipelineBuilder builder, TInputOptions inputOptions) : this(builder, new TInputOptions(), new List<TOutputOptions>())
        {
        }

        public BaseRollupPipeOptionsBuilder(IPipelineBuilder builder, TInputOptions inputOptions, List<TOutputOptions> outputOptions)
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
            InputOptions.AddPlugin(builder.ModuleRequirement.PackageName, builder.GetJsonConfigurationObject(), builder.DefaultExportName, builder.IsImportOnly, builder.PluginRunsBeforeSystemPlugins);
            return (TBuilder)this;
        }

        public TBuilder AddImport(Action<IRollupImportOptionsBuilder> configurePlugin)
        {
            RollupImportOptionsBuilder builder = new RollupImportOptionsBuilder();
            configurePlugin(builder);
            IPipelineBuilder.IncludeRequirement(builder.ModuleRequirement);
            InputOptions.AddPlugin(builder.ModuleRequirement.PackageName, null, builder.DefaultExportName, true);
            return (TBuilder)this;
        }

        public TBuilder AddOutput(Action<TOutputOptions> configureOutput)
        {
            var output = new TOutputOptions();
            configureOutput(output);
            OutputOptions.Add(output);
            return (TBuilder)this;
        }

        public IPipelineBuilder IPipelineBuilder => _builder;

        public TInputOptions InputOptions => _inputOptions;

        public List<TOutputOptions> OutputOptions => _outputOptions;

    }
}