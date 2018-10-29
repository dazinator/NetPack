using Newtonsoft.Json.Linq;

namespace NetPack.Rollup
{
    public class RollupPlugin
    {
        private readonly string _packageName;
        private readonly JObject _pluginConfiguration;
        private readonly bool _importOnly;
        private readonly string _defaultExportName;
        

        public RollupPlugin(string packageName, JObject configuration, string defaultExportName, bool importOnly)
        {
            if (defaultExportName == null)
            {
               // defaultExportName = //"plugin" + Guid.NewGuid().ToString("N");
            }
            _packageName = packageName;
            _pluginConfiguration = configuration;
            _importOnly = importOnly;
            _defaultExportName = defaultExportName;
        }

        public string PackageName => _packageName;

        public JObject PluginConfiguration => _pluginConfiguration;

        public string PluginConfigurationJson
        {
            get
            {
                if (PluginConfiguration == null)
                {
                    return null;
                }

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(PluginConfiguration)
                    .Replace("\"FUNC", "").Replace("FUNC\"", ""); // hack to allow javascript functions to be sent via json see: https://stackoverflow.com/questions/4901859/send-javascript-function-over-json-using-json-net-lib
                return json;
            }
        }

        public bool ImportOnly => _importOnly;

        public string DefaultExportName => _defaultExportName;
    }
}