using NetPack.Requirements;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack.Requirements
{

    public class NpmDependencyBuilder : INpmDependencyBuilder
    {

        public NpmDependencyBuilder()
        {
        }

        private string _version;
        private string _packageName;

        protected string Version { get => _version; set => _version = value; }
        protected string PackageName { get => _packageName; set => _packageName = value; }

        public NpmDependencyBuilder SetVersion(string version)
        {
            Version = version;
            return this;
        }

        public NpmDependencyBuilder SetPackageName(string name)
        {
            PackageName = name;
            return this;
        }

        public virtual NpmDependency BuildRequirement()
        {
            var dep = new NpmDependency(PackageName, Version);
            return dep;
        }

    }
}