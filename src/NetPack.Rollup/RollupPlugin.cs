using Newtonsoft.Json.Linq;

namespace NetPack.Rollup
{
    public class RollupPlugin
    {
        private readonly string _packageName;
        private readonly JObject _moduleConfiguration;

        public RollupPlugin(string packageName) : this(packageName, null)
        {
        }

        public RollupPlugin(string packageName, JObject configuration)
        {
            _packageName = packageName;
            _moduleConfiguration = configuration;
        }

    }

}