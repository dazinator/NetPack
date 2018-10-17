using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NetPack.Rollup
{
    public class RollupInputOptions
    {

        public RollupInputOptions()
        {
            Plugins = new List<RollupPlugin>();          
        }

        public void AddPlugin(string name, JObject configuration)
        {
            Plugins.Add(new RollupPlugin(name, configuration));
        }

        public List<RollupPlugin> Plugins { get; set; }

        public string Input { get; set; }


    }

}