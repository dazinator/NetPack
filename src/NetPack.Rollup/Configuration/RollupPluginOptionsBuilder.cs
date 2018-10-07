using NetPack.Requirements;
using Newtonsoft.Json.Linq;
using System;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public class RollupPluginOptionsBuilder : IRollupPluginOptionsBuilder
    {
        private JObject _options = null;
        private RollupPipeOptionsBuilder _builder;
        private NpmModuleRequirement _moduleRequirement = null;
       

        public RollupPluginOptionsBuilder(RollupPipeOptionsBuilder builder)
        {
            _builder = builder;
        }

        public RollupPluginOptionsBuilder RequiresNpmModule(Action<NpmModuleRequirementBuilder> configureNpmModuleRequirement)
        {
            NpmModuleRequirementBuilder builder = new NpmModuleRequirementBuilder();
            configureNpmModuleRequirement?.Invoke(builder);
            _moduleRequirement = builder.BuildRequirement();          
            return this;
        }

        public RollupPluginOptionsBuilder RequiresNpmModule(string packageName, string version, bool installAutomatically = true)
        {
            var module = new NpmModuleRequirement(packageName, installAutomatically, version);
            _moduleRequirement = module;
            return this;
        }


        public RollupPipeOptionsBuilder WithConfiguration(Action<dynamic> configure)
        {
            JObject options = new JObject();
            configure?.Invoke(options);
            _options = options;
            return _builder;
        }

        public RollupPipeOptionsBuilder Build()
        {
            _builder.IPipelineBuilder.IncludeRequirement(_moduleRequirement);
            _builder.RollupPipeOptions.AddPlugin(_moduleRequirement.PackageName, _options);
            return _builder;
        }       
    }
}