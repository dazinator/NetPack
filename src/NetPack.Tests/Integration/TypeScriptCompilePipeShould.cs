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
using Dazinator.AspNet.Extensions.FileProviders.Directory;

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

        private async Task<string> GetResponseString(string path, string querystring = "")
        {
            var request = path;
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
            var responseString = await GetResponseString("/netpack/wwwroot/somefile.js");
            Assert.False(string.IsNullOrWhiteSpace(responseString));

            // because we have source maps enabled, we should also be able to resolve the sourcemap, as well as the original source file.
            responseString = await GetResponseString("/netpack/wwwroot/somefile.js.map");
            Assert.False(string.IsNullOrWhiteSpace(responseString));


            responseString = await GetResponseString("/netpack/wwwroot/somefile.ts");
            Assert.False(string.IsNullOrWhiteSpace(responseString));

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

            app.UseStaticFiles(new StaticFileOptions() { });

            var inputFileProvider = new InMemoryFileProvider();
            inputFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(TsContentOne, "somefile.ts"));

            app.UseFileProcessing(a =>
            {
                a.WithFileProvider(inputFileProvider)
                    // Simple processor, that compiles typescript files.
                    .AddTypeScriptPipe(input =>
                    {
                        input.Include("wwwroot/*.ts");
                    }, options =>
                    {
                        // configure various typescript compilation options here..
                        options.SourceMap = true;
                        // options.InlineSourceMap = true;
                        //  options.Module = ModuleKind.Amd;
                    })
                    .UseBaseRequestPath("netpack")

                    // allows the files produced from processing to be resolved via the environemtns webroot file provider..
                    .Watch(); // Input files are watched, and when changes occur, pipeline will automatically trigger necessary processing.
                    
            });

            //   app.Run(async (context) =>
            // {
            //var nodeServices = context.RequestServices.GetService<INodeServices>();
            //var nodeJsRequirement = context.RequestServices.GetService<NodeJsRequirement>();
            //var embeddedResourceProvider = context.RequestServices.GetService<IEmbeddedResourceProvider>();

            //var pipe = new TypeScriptCompilePipe(nodeServices, embeddedResourceProvider);


            //// inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(AmdModuleAFileContent, "moduleB.js"));

            //// When source maps are encountered, the processor need to ensure thats source files being processed (such as typescript files etc)
            //// can be served up to the browser. The following directory is where such source files will be added by the processor, so we can
            //// check after processing that it has added the correct source files.
            //IDirectory sourcesOutputDirectory = new InMemoryDirectory();

            //var pipelineContext = new PipelineContext(inputFileProvider, sourcesOutputDirectory);
            //var input = new PipelineInput();
            //input.AddInclude("wwwroot/*.ts");
            //pipelineContext.SetInput(input);
            //  var pipelineContext = new PipelineContext();
            //  pipelineContext.InputFiles.Add(new SourceFile(new StringFileInfo(TsContentOne, "somefile.ts"), "wwwroot"));

            //await pipe.ProcessAsync(pipelineContext, CancellationToken.None);

            //var builder = new StringBuilder();

            //foreach (var output in pipelineContext.ProcessedOutput.GetFolder("wwwroot"))
            //{
            //    using (var reader = new StreamReader(output.FileInfo.CreateReadStream()))
            //    {
            //        builder.AppendLine("File Name: " + output.Path.ToString());
            //        builder.Append(reader.ReadToEnd());
            //    }
            //}

            //await context.Response.WriteAsync(builder.ToString());

            //   });

        }
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddNetPack();
        }
    }
}