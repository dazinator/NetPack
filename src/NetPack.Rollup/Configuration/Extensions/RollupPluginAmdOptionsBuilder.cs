using Newtonsoft.Json.Linq;

namespace NetPack
{
    public static partial class RollupConfigExtensions
    {
        public class RollupPluginAmdOptionsBuilder : BaseOptionsBuilder<RollupPluginAmdOptionsBuilder>
        {
            public RollupPluginAmdOptionsBuilder(JObject options) : base(options)
            {
            }

            public RollupPluginAmdOptionsBuilder RewireFunction(string function)
            {
                return AddFunctionProperty("rewire", function);
            }

        }


    }
}
