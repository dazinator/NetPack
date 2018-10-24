using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NetPack.Rollup
{
    public class BaseRollupInputOptions
    {
        public BaseRollupInputOptions()
        {
            Plugins = new List<RollupPlugin>();
        }

        public void AddPlugin(string name, JObject configuration, string defaultExportName = null, bool importOnly = false)
        {
            Plugins.Add(new RollupPlugin(name, configuration, defaultExportName, importOnly));
        }

        public List<RollupPlugin> Plugins { get; set; }
    }

}