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

        [JsonConverter(typeof(StringEnumConverter))]
        public RollupOutputFormat Format { get; set; }

    }
}