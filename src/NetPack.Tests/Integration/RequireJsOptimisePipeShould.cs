using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.AspNetCore.TestHost;
using NetPack.Pipes;
using NetPack.Requirements;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Utils;

namespace NetPack.Tests.Integration
{
    public class RequireJsOptimisePipeShould
    {

        private readonly TestServer _server;
        private readonly HttpClient _client;

        public RequireJsOptimisePipeShould()
        {
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
        public async void Processes_TypescriptFiles_And_Outputs_Js_Files()
        {

            // Act
            var responseString = await GetResponseString();

            // Assert
            //   Assert.Equal("Pass in a number to check in the form /checkprime?5", responseString);

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
define(""ModuleB"", [""require"", ""exports"", ""ModuleA""], function (require, exports, moduleB) {
    ""use strict"";
});

";

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {


            app.Run(async (context) =>
            {
                var nodeServices = context.RequestServices.GetService<INodeServices>();
                var embeddedResourceProvider = context.RequestServices.GetService<IEmbeddedResourceProvider>();

                var sut = new RequireJsOptimisePipe(nodeServices, embeddedResourceProvider);

                // Provide some AMD modules as input for the RequireJs Optimise Pipe
                var pipelineContext = new PipelineContext();
                pipelineContext.InputFiles.Add(new SourceFile(new StringFileInfo(AmdModuleAFileContent, "moduleA.js"), "wwwroot"));
                pipelineContext.InputFiles.Add(new SourceFile(new StringFileInfo(AmdModuleBFileContent, "moduleB.js"), "wwwroot"));

                await sut.ProcessAsync(pipelineContext);

                // Write the content of any outputs to the response for inspection.
                var builder = new StringBuilder();

                foreach (var output in pipelineContext.OutputFiles)
                {
                    using (var reader = new StreamReader(output.FileInfo.CreateReadStream()))
                    {
                        builder.AppendLine("File Name: " + output.FileInfo.Name + " Directory: " + output.Directory);
                        builder.Append(reader.ReadToEnd());
                    }
                }

                await context.Response.WriteAsync(builder.ToString());


            });

        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNetPack();
        }
    }
}