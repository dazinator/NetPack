using System.Collections.Generic;

namespace NetPack.Requirements
{
    public class NpmDependency
    {
        public string PackageName { get; private set; }

        public string Version { get; private set; } = null;

        public NpmDependency(string packageName, string version = null)
        {
            PackageName = packageName;
            Version = version;
        }      

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() == typeof(NpmDependency))
            {
                NpmDependency req = (NpmDependency)obj;
                return req.PackageName == PackageName && req.Version == Version;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 2131659351;
            var defaultComparer = EqualityComparer<string>.Default;

            hashCode = hashCode * -1521134295 + defaultComparer.GetHashCode(PackageName);
            hashCode = hashCode * -1521134295 + defaultComparer.GetHashCode(Version);
            return hashCode;
        }
    }
}