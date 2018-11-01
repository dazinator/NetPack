using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetPack.Rollup
{
    public class BaseRollupOutputOptions
    {
        public BaseRollupOutputOptions()
        {
            Format = RollupOutputFormat.System;            
        }

        /// <summary>
        /// The format of the generated bundle.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RollupOutputFormat Format { get; set; }

        public SourceMapType? Sourcemap { get; set; }

        /// <summary>
        /// The variable name, representing your iife/umd bundle, by which other scripts on the same page can access it.
        /// </summary>
        public string Name { get; set; }

    }

}