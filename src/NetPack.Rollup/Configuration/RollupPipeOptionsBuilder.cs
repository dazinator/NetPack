using NetPack.Pipeline;
using NetPack.Rollup;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{

    public class RollupPipeOptionsBuilder : BaseRollupPipeOptionsBuilder<RollupInputOptions, RollupOutputFileOptions, RollupPipeOptionsBuilder>
    {

        public RollupPipeOptionsBuilder(IPipelineBuilder builder) : this(builder, new RollupInputOptions())
        {
        }

        public RollupPipeOptionsBuilder(IPipelineBuilder builder, RollupInputOptions inputOptions) : this(builder, new RollupInputOptions(), new RollupOutputFileOptions())
        {
        }

        public RollupPipeOptionsBuilder(IPipelineBuilder builder, RollupInputOptions inputOptions, RollupOutputFileOptions outputOptions) : base(builder, inputOptions, outputOptions)
        {
        }

    }
}