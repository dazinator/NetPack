using NetPack.Pipeline;
using NetPack.Rollup;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public class RollupPipeCodeSplittingOptionsBuilder : BaseRollupPipeOptionsBuilder<RollupCodeSplittingInputOptions, RollupPipeCodeSplittingOptionsBuilder>
    {
        private readonly IPipelineBuilder _builder;
        private readonly RollupCodeSplittingInputOptions _inputOptions;
        private readonly RollupOutputOptions _outputOptions;
               


        public RollupPipeCodeSplittingOptionsBuilder(IPipelineBuilder builder) : this(builder, new RollupCodeSplittingInputOptions())
        {
        }

        public RollupPipeCodeSplittingOptionsBuilder(IPipelineBuilder builder, RollupCodeSplittingInputOptions inputOptions) : this(builder, new RollupCodeSplittingInputOptions(), new RollupOutputOptions())
        {
        }

        public RollupPipeCodeSplittingOptionsBuilder(IPipelineBuilder builder, RollupCodeSplittingInputOptions inputOptions, RollupOutputOptions outputOptions): base(builder,inputOptions,outputOptions)
        {
        }   

        
      
    }
}