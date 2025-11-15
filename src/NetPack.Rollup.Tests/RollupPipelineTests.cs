using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Dazinator.Extensions.FileProviders.InMemory;
using Xunit;
using Xunit.Abstractions;

namespace NetPack.Rollup.Tests
{

    public class RollupPipelineTests
    {

        private readonly ITestOutputHelper _output;


        public RollupPipelineTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Can_Be_Configured()
        {

            ServiceCollection services = new ServiceCollection();
            InMemoryFileProvider fileProvider = new InMemoryFileProvider();

            services.AddNetPack((setup) =>
            {
                setup.AddPipeline(a =>
                {
                    a.WithFileProvider(fileProvider)
                        .AddRollupPipe(input =>
                        {
                            input.Include("wwwroot/*.js");
                        }, options =>
                        {
                            options.AddPlugin((plugin) => plugin.HasNpmDependency("foo", "1.0.0")
                                                                .HasOptionsOfKind(OptionsKind.Object, (c) =>
                                                                {
                                                                    c.Prop = true;
                                                                }));
                        });
                });
            });
        }
    }
}