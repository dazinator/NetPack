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
    [UseReporter(typeof(DiffReporter))]
    public class RollupScriptGeneratorTests
    {

        private readonly ITestOutputHelper _output;


        public RollupScriptGeneratorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Can_Generate_Script()
        {

            var rollupOptions = new RollupPipeOptions();
            dynamic pluginOptions = new Newtonsoft.Json.Linq.JObject();
            pluginOptions.Start = true;
            pluginOptions.Times = 1;
            pluginOptions.Things = new JArray("foo", "bar");
            rollupOptions.AddPlugin("foopackage", pluginOptions);
            rollupOptions.AddPlugin("barpackage", null);

            var path = "./RollupTemplate.txt";
            var templateText = System.IO.File.ReadAllText(path);
            var template = Template.Parse(templateText);
            var result = template.Render(rollupOptions);

            Approvals.Verify(result);

        }     
    }
}