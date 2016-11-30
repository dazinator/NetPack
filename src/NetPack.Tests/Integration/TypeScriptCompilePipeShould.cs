using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
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
using NetPack.Pipes.Typescript;
using NetPack.Utils;
using Dazinator.AspNet.Extensions.FileProviders;

namespace NetPack.Tests.Integration
{
    public class TypeScriptCompilePipeShould
    {

        private readonly TestServer _server;
        private readonly HttpClient _client;

        public TypeScriptCompilePipeShould()
        {
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        private async Task<string> GetResponseString(string querystring = "")
        {
            var request = "/compilets";
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

    public class Startup
    {

        public const string TsContentOne = @"

        class Greeter
        {
    constructor(public greeting: string) { }
    greet()
            {
                return ""<h1>"" + this.greeting + ""</h1>"";
            }
        };

        var greeter = new Greeter(""Hello, world!"");

        document.body.innerHTML = greeter.greet();";

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {


            app.Run(async (context) =>
            {
                var nodeServices = context.RequestServices.GetService<INodeServices>();
                var nodeJsRequirement = context.RequestServices.GetService<NodeJsRequirement>();
                var embeddedResourceProvider = context.RequestServices.GetService<IEmbeddedResourceProvider>();
              
                var pipe = new TypeScriptCompilePipe(nodeServices, embeddedResourceProvider);

                var inMemoryFileProvider = new InMemoryFileProvider();
                inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(TsContentOne, "somefile.ts"));
               // inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(AmdModuleAFileContent, "moduleB.js"));

                var pipelineContext = new PipelineContext(inMemoryFileProvider);
                var input = new PipelineInput();
                input.IncludeList.Add("wwwroot/*.js");
                var files = input.GetFiles(inMemoryFileProvider);
                //  var pipelineContext = new PipelineContext();
                //  pipelineContext.InputFiles.Add(new SourceFile(new StringFileInfo(TsContentOne, "somefile.ts"), "wwwroot"));

                await pipe.ProcessAsync(pipelineContext, files, CancellationToken.None);

                var builder = new StringBuilder();

                foreach (var output in pipelineContext.Output.GetFolder("wwwroot"))
                {
                    using (var reader = new StreamReader(output.FileInfo.CreateReadStream()))
                    {
                        builder.AppendLine("File Name: " + output.Path.ToString());
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