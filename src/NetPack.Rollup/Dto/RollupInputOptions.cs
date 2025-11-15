using System.Text.Json.Serialization;

namespace NetPack.Rollup
{

    public class RollupInputOptions : BaseRollupInputOptions
    {    
        [JsonPropertyName("input")]
        public string Input { get; set; }
    }  

}