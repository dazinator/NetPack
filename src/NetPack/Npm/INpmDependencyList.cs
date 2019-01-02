using System;

namespace NetPack.Requirements
{
    public interface INpmDependencyList
    {
        INpmDependencyList AddDependency(NpmDependency package);
        INpmDependencyList AddDependency(string packageName, string version = null);
        INpmDependencyList AddDependency(Action<INpmDependencyBuilder> configure);
        bool Any();
    }
}