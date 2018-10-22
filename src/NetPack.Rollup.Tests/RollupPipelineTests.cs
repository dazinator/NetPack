using ApprovalTests;
using ApprovalTests.Reporters;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Scriban;
using System.Threading.Tasks;
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
                            options.AddPlugin((plugin) => plugin.RequiresNpmModule("foo", "1.0.0")
                                                                .Register((dynamic c) =>
                                                                 {
                                                                     c.Prop = true;
                                                                 }));
                        });
                });
            });
        }  
    }
}