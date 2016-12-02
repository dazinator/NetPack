using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.AspNet.Extensions.FileProviders;
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
        public async void Optimise_Specified_Js_Files()
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
define(""ModuleB"", [""require"", ""exports"", ""ModuleA""], function (require, exports, moduleA) {
    ""use strict"";
});

";

        public const string ConfigFileContent =
            @"requirejs.config({\r\n baseUrl: \'wwwroot\',\r\n    paths: {\r\n        app: \'..\/app\'\r\n    }\r\n});";



        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //var nodeServices = app.ApplicationServices.GetService<INodeServices>();
            //var embeddedResourceProvider = app.ApplicationServices.GetService<IEmbeddedResourceProvider>();
            //var sut = new RequireJsOptimisePipe(nodeServices, embeddedResourceProvider);

            // Provide some AMD modules as input for the RequireJs Optimise Pipe
            var inMemoryFileProvider = new InMemoryFileProvider();
            inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(AmdModuleAFileContent, "ModuleA.js"));
            inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(AmdModuleBFileContent, "ModuleB.js"));
            inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(ConfigFileContent, "Common.js"));

            var fileProcessingBuilder = app.UseFileProcessing(a =>
             {
                 a.WithFileProvider(inMemoryFileProvider)
                     .AddRequireJsOptimisePipe(input =>
                     {
                         input.Include("wwwroot/*.js");
                     }, options =>
                     {
                        // options.Out = "build.js";
                       //  options.AppDir = ".";
                         options.BaseUrl = "wwwroot";
                         // options.BaseUrl = "wwwroot"; // append request path
                         options.Dir = "built";
                         options.Modules.Add(new ModuleInfo() { Name = "ModuleA" });
                     });
             });

            var pipeline = fileProcessingBuilder.Pipeline;
            pipeline.Initialise();

            app.Run(async (context) =>
            {



                // var pipelineContext = new PipelineContext(inMemoryFileProvider);
                // pipelineContext.InputFiles.Add(new SourceFile(, "wwwroot"));
                // pipelineContext.InputFiles.Add(new SourceFile(new StringFileInfo(AmdModuleBFileContent, "moduleB.js"), "wwwroot"));
                //  var input = new PipelineInput();
                // input.IncludeList.Add("wwwroot/*.js");
                // var files = input.GetFiles(inMemoryFileProvider);

                // await sut.ProcessAsync(pipelineContext, files, CancellationToken.None);

                // Write the content of any outputs to the response for inspection.
                var builder = new StringBuilder();
                var outputFolder = "wwwroot";
                foreach (var outputFile in pipeline.OutputFileProvider.GetDirectoryContents(outputFolder))
                {
                    using (var reader = new StreamReader(outputFile.CreateReadStream()))
                    {
                        builder.AppendLine("File: " + outputFolder + "/" + outputFile.Name);
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