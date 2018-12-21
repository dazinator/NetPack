using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NetPack.Rollup
{
    public class BaseRollupInputOptions
    {
        public BaseRollupInputOptions()
        {
            Plugins = new List<RollupPlugin>();
            External = new List<string>();
        }

        public void AddPlugin(string name, object configuration, string defaultExportName = null, bool importOnly = false, bool addBeforeVirtualFileSystem = false)
        {
            Plugins.Add(new RollupPlugin(name, configuration, defaultExportName, importOnly, addBeforeVirtualFileSystem));
        }

        /// <summary>
        /// List of plugins that will participate in the rollup bundling process.
        /// </summary>
        public List<RollupPlugin> Plugins { get; set; }

        /// <summary>
        ///  A List of module IDs that should remain external to the bundle.
        /// </summary>
        public List<string> External { get; set; }

        public BaseRollupInputOptions AddExternal(string externalModuleName)
        {
            External.Add(externalModuleName);
            return this;
        }

    }

}