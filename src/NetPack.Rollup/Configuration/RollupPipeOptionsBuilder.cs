using NetPack.Pipeline;
using NetPack.Rollup;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{

    public class RollupPipeOptionsBuilder : BaseRollupPipeOptionsBuilder<RollupInputOptions, RollupPipeOptionsBuilder>
    {

        public RollupPipeOptionsBuilder(IPipelineBuilder builder) : this(builder, new RollupInputOptions())
        {
        }

        public RollupPipeOptionsBuilder(IPipelineBuilder builder, RollupInputOptions inputOptions) : this(builder, new RollupInputOptions(), new RollupOutputOptions())
        {
        }

        public RollupPipeOptionsBuilder(IPipelineBuilder builder, RollupInputOptions inputOptions, RollupOutputOptions outputOptions) : base(builder, inputOptions, outputOptions)
        {
        }

    }
}