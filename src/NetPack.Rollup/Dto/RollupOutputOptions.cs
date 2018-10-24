using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetPack.Rollup
{
    public class RollupOutputOptions
    {

        public RollupOutputOptions()
        {
            Format = RollupOutputFormat.System;
            File = "bundle.js";
        }

        [JsonConverter(typeof(StringEnumConverter))]        
        public RollupOutputFormat Format { get; set; }

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