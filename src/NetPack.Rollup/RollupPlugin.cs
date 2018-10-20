using Newtonsoft.Json.Linq;

namespace NetPack.Rollup
{
    public class RollupPlugin
    {
        private readonly string _packageName;      
        private readonly JObject _pluginConfiguration;

        public RollupPlugin(string packageName) : this(packageName, null)
        {
        }

        public RollupPlugin(string packageName, JObject configuration)
        {
            _packageName = packageName;
            _pluginConfiguration = configuration;
        }

        public string PackageName => _packageName;

        public JObject PluginConfiguration => _pluginConfiguration;

        public string PluginConfigurationJson => PluginConfiguration?.ToString();
    }

}