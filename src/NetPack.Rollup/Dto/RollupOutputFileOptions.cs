using System.Text.Json.Serialization;

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
        [JsonPropertyName("file")]
        public string File { get; set; }

    }

}