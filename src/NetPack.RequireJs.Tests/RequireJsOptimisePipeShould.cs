using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dazinator.Extensions.FileProviders;
using Dazinator.Extensions.FileProviders.InMemory;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Http;
using NetPack.Tests.Utils;

namespace NetPack.RequireJs.Tests
{
    public class RequireJsOptimisePipeShould
    {

        private readonly TestServer _server;
        private readonly HttpClient _client;

        public RequireJsOptimisePipeShould()
        {
            NodeFnmHelper.SetPath();
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<RequireJsOptimisePipeTestsStartup>());
            _client = _server.CreateClient();
        }

        private async Task<string> GetResponseString(string querystring = "")
        {
            var request = "/run";
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
            var responseString = await GetResponseString();
            Assert.Contains("File: built/ModuleA.js", responseString);
        }
    }

    public class RequireJsOptimisePipeTestsStartup
    {

        public const string AmdModuleAFileContent = @"

       define(""ModuleA"", [""require"", ""exports""], function (require, exports) {
    ""use strict"";
});

";

        public const string AmdModuleBFileContent = @"
define(""ModuleB"", [""require"", ""exports"", ""ModuleA""], function (require, exports, moduleA) {
    ""use strict"";
});

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

                foreach (var outputFile in env.WebRootFileProvider.GetDirectoryContents("built"))
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
                var outputFolder = "built";
                var fileProcessingBuilder = setup.AddPipeline(a =>
                {
                    a.WithFileProvider(inMemoryFileProvider)
                        .AddRequireJsOptimisePipe(input =>
                        {
                            input.Include("wwwroot/*.js");
                        }, options =>
                        {
                            options.BaseUrl = "wwwroot";
                            options.Dir = outputFolder;
                            options.Modules.Add(new ModuleInfo() { Name = "ModuleA" });
                        });
                });

            });
        }
    }
}