using NetPack.Requirements;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack.Requirements
{

    public class NpmModuleRequirementBuilder : INpmModuleRequirementBuilder
    {

        public NpmModuleRequirementBuilder()
        {
        }

        private string _version;
        private string _packageName;
        private bool _installIfNotFound = false;

        protected string Version { get => _version; set => _version = value; }
        protected string PackageName { get => _packageName; set => _packageName = value; }
        protected bool InstallIfNotFound { get => _installIfNotFound; set => _installIfNotFound = value; }

        public NpmModuleRequirementBuilder SetVersion(string version)
        {
            Version = version;
            return this;
        }

        public NpmModuleRequirementBuilder SetPackageName(string name)
        {
            PackageName = name;
            return this;
        }

        public NpmModuleRequirementBuilder InstallAutomatically()
        {
            InstallIfNotFound = true;
            return this;
        }

        public virtual NpmModuleRequirement BuildRequirement()
        {
            NpmModuleRequirement requirement = new NpmModuleRequirement(PackageName, InstallIfNotFound, Version);
            return requirement;
            // return _builder;
        }

    }
}