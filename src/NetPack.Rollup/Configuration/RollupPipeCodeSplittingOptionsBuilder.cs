using NetPack.Pipeline;
using NetPack.Rollup;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public class RollupPipeCodeSplittingOptionsBuilder : BaseRollupPipeOptionsBuilder<RollupCodeSplittingInputOptions, RollupOutputDirOptions, RollupPipeCodeSplittingOptionsBuilder>
    {
        public RollupPipeCodeSplittingOptionsBuilder(IPipelineBuilder builder) : this(builder, new RollupCodeSplittingInputOptions())
        {
        }

        public RollupPipeCodeSplittingOptionsBuilder(IPipelineBuilder builder, RollupCodeSplittingInputOptions inputOptions) : this(builder, new RollupCodeSplittingInputOptions(), new List<RollupOutputDirOptions>())
        {
        }

        public RollupPipeCodeSplittingOptionsBuilder(IPipelineBuilder builder, RollupCodeSplittingInputOptions inputOptions, List<RollupOutputDirOptions> outputOptions) : base(builder, inputOptions, outputOptions)
        {
        }
    }
}