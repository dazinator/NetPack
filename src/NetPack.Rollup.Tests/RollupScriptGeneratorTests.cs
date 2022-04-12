using ApprovalTests;
using ApprovalTests.Reporters;
using Dazinator.Extensions.FileProviders;
using Newtonsoft.Json.Linq;
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

            RollupInputOptions rollupOptions = new RollupInputOptions();
            dynamic pluginOptions = new Newtonsoft.Json.Linq.JObject();
            pluginOptions.Start = true;
            pluginOptions.Times = 1;
            pluginOptions.Things = new JArray("foo", "bar");

            rollupOptions.AddPlugin("foopackage", pluginOptions, "plugin0");
            rollupOptions.AddPlugin("barpackage", null, "plugin1");

            string path = "./RollupTemplate.txt";
            StringFileInfo templateFile = new StringFileInfo(System.IO.File.ReadAllText(path), "RollupTemplate.txt");
            RollupScriptGenerator generator = new RollupScriptGenerator(templateFile);
            string result = generator.GenerateScript(rollupOptions);
            Approvals.Verify(result);

        }


        [Fact]
        public async Task Can_Generate_Script_With_ImportOnly_Package()
        {

            RollupInputOptions rollupOptions = new RollupInputOptions();
            dynamic pluginOptions = new Newtonsoft.Json.Linq.JObject();
            pluginOptions.Start = true;
            pluginOptions.Times = 1;
            pluginOptions.Things = new JArray("foo", "bar");

            rollupOptions.AddPlugin("foopackage", pluginOptions);
            rollupOptions.AddPlugin("barpackage", null, "barp", true);

            string path = "./RollupTemplate.txt";
            StringFileInfo templateFile = new StringFileInfo(System.IO.File.ReadAllText(path), "RollupTemplate.txt");
            RollupScriptGenerator generator = new RollupScriptGenerator(templateFile);
            string result = generator.GenerateScript(rollupOptions);
            Approvals.Verify(result);

        }
    }
}