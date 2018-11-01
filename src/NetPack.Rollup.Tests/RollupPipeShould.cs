using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetPack.Rollup.Tests
{
    public class RollupPipeShould
    {

        private readonly TestServer _server;
        private readonly HttpClient _client;

        public RollupPipeShould()
        {
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<RollupPipeShouldTestsStartup>());
            _client = _server.CreateClient();
        }

        private async Task<string> GetResponseString(string querystring = "")
        {
            string request = "/run";
            if (!string.IsNullOrEmpty(querystring))
            {
                request += "?" + querystring;
            }

            var response = await _client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async Task Optimise_Specified_Js_Files()
        {
            // Act
            string responseString = await GetResponseString();
            Assert.Contains("File: built/bundle.js", responseString);
            Assert.Contains("File: built/bundle.js.map", responseString);
        }
    }

    public class RollupPipeShouldTestsStartup
    {
        public const string AmdModuleAFileContent = @"
export class ClassA {
    constructor(another) { }
    doSomething() {
        // return ""<h1>"" + this.greeting + ""</h1>"";
    }
}

var classA = new ClassA(""Hello, world!"");
classA.doSomething();

";

        public const string AmdModuleBFileContent = @"

      import {ClassA} from ""./ModuleA"";


export class ClassB
        {
            constructor(another) { }
            doSomething()
            {
                // return ""<h1>"" + this.greeting + ""</h1>"";
            }
        };

        var classB = new ClassB(""Hello, world!"");
        classB.doSomething();

var classA = new ClassA(""Hello, world!"");
        classA.doSomething();

";
        //public const string ConfigFileContent =
        //    @"requirejs.config({\r\n baseUrl: \'wwwroot\',\r\n    paths: {\r\n        app: \'..\/app\'\r\n    }\r\n});";

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            app.UseNetPack();



            //  var pipeline = fileProcessingBuilder.Pipeline;
            // pipeline.Initialise();

            app.Run(async (context) =>
            {
                // Write the content of outputs files to the response for inspection.
                var builder = new StringBuilder();

                foreach (Microsoft.Extensions.FileProviders.IFileInfo outputFile in env.WebRootFileProvider.GetDirectoryContents("built"))
                {
                    using (var reader = new StreamReader(outputFile.CreateReadStream()))
                    {
                        builder.AppendLine("File: " + "built" + "/" + outputFile.Name);
                        builder.Append(reader.ReadToEnd());
                        builder.AppendLine();
                    }
                }

                await context.Response.WriteAsync(builder.ToString());
            });

        }
        public void ConfigureServices(IServiceCollection services)
        {

            // Provide some in memory files, that are AMD modules to be used as input for the RequireJs Optimise Pipe
            var inMemoryFileProvider = new InMemoryFileProvider();
            inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(AmdModuleAFileContent, "ModuleA.js"));
            inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(AmdModuleBFileContent, "ModuleB.js"));

            services.AddNetPack((setup) =>
            {

                var fileProcessingBuilder = setup.AddPipeline(a =>
                {

                    a.WithFileProvider(inMemoryFileProvider)
                        .AddRollupPipe(input =>
                        {
                            input.Include("wwwroot/*.js");
                        }, options =>
                        {
                            options.InputOptions.Input = "/wwwroot/ModuleB.js";
                            options.OutputOptions.Format = Rollup.RollupOutputFormat.Esm;
                            options.OutputOptions.File = "bundle.js";
                            options.OutputOptions.Sourcemap =  SourceMapType.File;
                        })
                        .UseBaseRequestPath("/built");
                });

            });
        }
    }
}