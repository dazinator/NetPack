using NetPack.Requirements;
using Newtonsoft.Json.Linq;
using System;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public interface IRollupPluginStepConfigurationBuilder
    {
        IRollupPluginStepConfigurationBuilder Register(Action<dynamic> configure);
        IRollupPluginStepConfigurationBuilder ImportOnly();
        IRollupPluginStepConfigurationBuilder DefaultExportName(string name);

    }
    public class RollupPluginOptionsBuilder : IRollupPluginOptionsBuilder, IRollupPluginStepConfigurationBuilder
    {
        private JObject _options = null;
       // private RollupPipeOptionsBuilder _builder;
        private NpmModuleRequirement _moduleRequirement = null;
        private bool _importOnly = false;
        private string _defaultExportName = null;

        public NpmModuleRequirement ModuleRequirement { get => _moduleRequirement; set => _moduleRequirement = value; }
        public JObject Options { get => _options; set => _options = value; }
        public string DefaultExportName1 { get => _defaultExportName; set => _defaultExportName = value; }
        public bool IsImportOnly { get => _importOnly; set => _importOnly = value; }

        public RollupPluginOptionsBuilder()
        {
            
        }

        public IRollupPluginStepConfigurationBuilder RequiresNpmModule(Action<NpmModuleRequirementBuilder> configureNpmModuleRequirement)
        {
            NpmModuleRequirementBuilder builder = new NpmModuleRequirementBuilder();
            configureNpmModuleRequirement?.Invoke(builder);
            ModuleRequirement = builder.BuildRequirement();
            return this;
        }

        public IRollupPluginStepConfigurationBuilder RequiresNpmModule(string packageName, string version, bool installAutomatically = true)
        {
            NpmModuleRequirement module = new NpmModuleRequirement(packageName, installAutomatically, version);
            ModuleRequirement = module;
            return this;
        }


        public IRollupPluginStepConfigurationBuilder Register(Action<dynamic> configure = null)
        {
            IsImportOnly = false;
            if (configure != null)
            {
                JObject options = new JObject();
                configure?.Invoke(options);
                Options = options;
            }
            return this;
        }

        public IRollupPluginStepConfigurationBuilder ImportOnly()
        {
            IsImportOnly = true;
            return this;
        }

        public IRollupPluginStepConfigurationBuilder DefaultExportName(string name)
        {
            DefaultExportName1 = name;
            return this;
        }
       
    }
}