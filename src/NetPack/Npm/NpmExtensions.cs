using Microsoft.Extensions.DependencyInjection;
using NetPack.Requirements;
using System;

namespace NetPack.Pipeline
{
    public static class NpmExtensions
    {
        public static void DependsOnNode(this IPipelineBuilder builder, Action<INpmDependencyList> configureNpmDependencies)
        {

            var nodeJsRequirement = new NodeJsIsInstalledRequirement();
            builder.IncludeRequirement(nodeJsRequirement);

            var dependencies = builder.ServiceProvider.GetRequiredService<NpmDependencyList>();
            configureNpmDependencies?.Invoke(dependencies);

            var depsRequirement = new NpmDependenciesRequirement(dependencies);
            builder.IncludeRequirement(depsRequirement);
        }

    }
}