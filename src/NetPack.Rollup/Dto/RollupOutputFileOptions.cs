namespace NetPack.Rollup
{
    public class RollupOutputFileOptions : BaseRollupOutputOptions
    {
        public RollupOutputFileOptions()
        {
            File = "bundle.js";
        }

        /// <summary>
        /// The bundle file to be produced.
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// When using experimentalCodeSplitting, rather than a single file, multiple output files will be placed under this dir.
        /// </summary>
        public string Dir { get; set; }



    }

}