using System.Text.Json.Nodes;

namespace NetPack
{
    public static partial class RollupConfigExtensions
    {
        public class RollupPluginAmdOptionsBuilder : BaseOptionsBuilder<RollupPluginAmdOptionsBuilder>
        {
            public RollupPluginAmdOptionsBuilder(JsonObject options) : base(options)
            {
            }

            public RollupPluginAmdOptionsBuilder RewireFunction(string function)
            {
                return AddFunctionProperty("rewire", function);
            }

        }


    }
}
