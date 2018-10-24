using System.Collections.Generic;

namespace NetPack.Rollup
{
    public class RollupCodeSplittingInputOptions : BaseRollupInputOptions
    {
        public RollupCodeSplittingInputOptions() : base()
        {
            Input = new List<string>();
        }

        public RollupCodeSplittingInputOptions AddEntryPoint(string path)
        {
            Input.Add(path);
            return this;
        }

        public List<string> Input { get; set; }
    }

}