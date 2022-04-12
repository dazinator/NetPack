// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack.Requirements
{
    public interface INpmDependencyBuilder
    {
        NpmDependencyBuilder SetVersion(string version);
        NpmDependencyBuilder SetPackageName(string name);
        //  NpmModuleRequirementBuilder InstallAutomatically();
    }
}