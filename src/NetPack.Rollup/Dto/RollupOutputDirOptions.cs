namespace NetPack.Rollup
{
    public class RollupOutputDirOptions : BaseRollupOutputOptions
    {
        public RollupOutputDirOptions()
        {
            Dir = "/rollup";
        }      

        /// <summary>
        /// When using experimentalCodeSplitting, rather than a single file, multiple output files will be placed under this dir.
        /// </summary>
        public string Dir { get; set; }

    }

}