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
        private RollupPipeOptionsBuilder _builder;
        private NpmModuleRequirement _moduleRequirement = null;
        private bool _importOnly = false;
        private string _defaultExportName = null;

        public RollupPluginOptionsBuilder(RollupPipeOptionsBuilder builder)
        {
            _builder = builder;
        }

        public IRollupPluginStepConfigurationBuilder RequiresNpmModule(Action<NpmModuleRequirementBuilder> configureNpmModuleRequirement)
        {
            NpmModuleRequirementBuilder builder = new NpmModuleRequirementBuilder();
            configureNpmModuleRequirement?.Invoke(builder);
            _moduleRequirement = builder.BuildRequirement();
            return this;
        }

        public IRollupPluginStepConfigurationBuilder RequiresNpmModule(string packageName, string version, bool installAutomatically = true)
        {
            NpmModuleRequirement module = new NpmModuleRequirement(packageName, installAutomatically, version);
            _moduleRequirement = module;
            return this;
        }


        public IRollupPluginStepConfigurationBuilder Register(Action<dynamic> configure = null)
        {
            _importOnly = false;
            if (configure != null)
            {
                JObject options = new JObject();
                configure?.Invoke(options);
                _options = options;
            }
            return this;
        }

        public IRollupPluginStepConfigurationBuilder ImportOnly()
        {
            _importOnly = true;
            return this;
        }

        public IRollupPluginStepConfigurationBuilder DefaultExportName(string name)
        {
            _defaultExportName = name;
            return this;
        }


        public RollupPipeOptionsBuilder Build()
        {
            _builder.IPipelineBuilder.IncludeRequirement(_moduleRequirement);
            _builder.InputOptions.AddPlugin(_moduleRequirement.PackageName, _options, _defaultExportName, _importOnly);
            return _builder;
        }
    }
}