using NetPack.Requirements;
using System;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{
    public interface IRollupPluginOptionsBuilder
    {
        IRollupPluginStepConfigurationBuilder HasNpmDependency(string name, string version);
        IRollupPluginStepConfigurationBuilder HasNpmDependency(Action<NpmDependencyBuilder> configureNpmModuleRequirement);
        //  RollupPipeOptionsBuilder WithConfiguration(Action<dynamic> configure);
    }

    public interface IRollupImportOptionsBuilder
    {
        IRollupImportConfigurationBuilder HasNpmDependency(string name, string version);
        IRollupImportConfigurationBuilder HasNpmDependency(Action<NpmDependencyBuilder> configureNpmModuleRequirement);
        //  RollupPipeOptionsBuilder WithConfiguration(Action<dynamic> configure);
    }
}