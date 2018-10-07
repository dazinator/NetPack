using NetPack.Requirements;
using System;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public interface IRollupPluginOptionsBuilder
    {
        RollupPluginOptionsBuilder RequiresNpmModule(string name, string version, bool installAutomatically = true);
        RollupPluginOptionsBuilder RequiresNpmModule(Action<NpmModuleRequirementBuilder> configureNpmModuleRequirement);
        RollupPipeOptionsBuilder WithConfiguration(Action<dynamic> configure);
    }
}