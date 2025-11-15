using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace NetPack.Requirements
{
    public class NpmDependencyList : INpmDependencyList
    {
        private readonly List<NpmDependency> _dependencies;

        public NpmDependencyList()
        {
            _dependencies = new List<NpmDependency>();
        }

        public INpmDependencyList AddDependency(NpmDependency package)
        {
            _dependencies.Add(package);
            return this;
        }

        public INpmDependencyList AddDependency(string packageName, string version = null)
        {
            _dependencies.Add(new NpmDependency(packageName, version));
            return this;
        }        

        public INpmDependencyList AddDependency(Action<INpmDependencyBuilder> configure)
        {
            NpmDependencyBuilder builder = new NpmDependencyBuilder();
            configure?.Invoke(builder);
            NpmDependency dep = builder.BuildRequirement();
            AddDependency(dep);
            return this;
        }

        public bool Any()
        {
            return _dependencies.Any();
        }
        
        public JsonObject ToJsonObject()
        {
            var deps = new JsonObject();

            foreach (var item in _dependencies)
            {
                deps[item.PackageName] = JsonValue.Create(item.Version);
            }

            return deps;
        }

        // public JObject ToJObject()
        // {
        //     JObject deps = new JObject();
        //
        //     foreach (var item in _dependencies)
        //     {
        //         deps[item.PackageName] = item.Version;
        //     }
        //
        //     return deps;
        // }
    }
}