// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack.Requirements
{
    public interface INpmModuleRequirementBuilder
    {
        NpmModuleRequirementBuilder SetVersion(string version);
        NpmModuleRequirementBuilder SetPackageName(string name);
        NpmModuleRequirementBuilder InstallAutomatically();
    }
}