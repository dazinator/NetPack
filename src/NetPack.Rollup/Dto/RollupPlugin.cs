using System.Text.Json;

namespace NetPack.Rollup
{
    public class RollupPlugin
    {
        private readonly string _packageName;
        private readonly object _pluginConfiguration;
        private readonly bool _importOnly;
        private readonly bool _addBeforeVirtualFileSystem;
        private readonly string _defaultExportName;
        

        public RollupPlugin(string packageName, object configuration, string defaultExportName, bool importOnly, bool addBeforeVirtualFileSystem)
        {
            if (defaultExportName == null)
            {
               // defaultExportName = //"plugin" + Guid.NewGuid().ToString("N");
            }
            _packageName = packageName;
            _pluginConfiguration = configuration;
            _importOnly = importOnly;
            _addBeforeVirtualFileSystem = addBeforeVirtualFileSystem;
            _defaultExportName = defaultExportName;
        }

        public string PackageName => _packageName;

        public object PluginConfiguration => _pluginConfiguration;

        public string PluginConfigurationJson
        {
            get
            {
                if (PluginConfiguration == null)
                {
                    return null;
                }

                string json =  JsonSerializer.Serialize(PluginConfiguration)
                    .Replace("\"FUNC", "").Replace("FUNC\"", ""); // hack to allow javascript functions to be sent via json see: https://stackoverflow.com/questions/4901859/send-javascript-function-over-json-using-json-net-lib
                return json;
            }
        }

        public bool ImportOnly => _importOnly;

        public bool AddBeforeVirtualFileSystem => _addBeforeVirtualFileSystem;

        public string DefaultExportName => _defaultExportName;
    }
}