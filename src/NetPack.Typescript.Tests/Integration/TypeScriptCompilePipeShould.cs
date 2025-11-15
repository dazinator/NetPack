using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using Dazinator.Extensions.FileProviders;
using Dazinator.Extensions.FileProviders.InMemory;
using Microsoft.AspNetCore.Http;
using NetPack.Tests.Utils;
using NetPack.Utils;

namespace NetPack.Typescript.Tests
{
    public class TypeScriptCompilePipeShould
    {

        private readonly TestServer _server;
        private readonly HttpClient _client;

        public TypeScriptCompilePipeShould()
        {
            NodeFnmHelper.SetPath();
            
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


        [Fact]
        public async void Send_Files_Incrementally_For_Build()
        {
            // Act
            // ge tthe current combined output file.
            var responseString = await GetResponseString("/netpack/combined.js");
            Assert.False(string.IsNullOrWhiteSpace(responseString));

            // change foo.ts, should result in an updated combined.js file being output -
            // but also - only foo.ts should be sent over to node as it was the only file that changes.
            await GetResponseString("/", "change=incremental/foo.ts");

            await Task.Delay(new TimeSpan(0, 0, 3));

            var updatedOutputFile = await GetResponseString("/netpack/combined.js");
            Assert.NotEqual(responseString, updatedOutputFile);


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


        public const string TsContentFoo = @"

        export class Foo
        {
    constructor(public greeting: string) { }
    greet()
            {
                return ""<h1>"" + this.greeting + ""</h1>"";
            }
        };

        var foo = new Foo(""Hello, world!"");

        document.body.innerHTML = foo.greet();";


        public const string TsContentBar = @"

         ///<reference path=""./foo.ts"" />

        import {Foo} from ""./foo"";
        class Bar
        {
    constructor(public greeting: string) { }
    greet()
            {
                return ""<h1>"" + this.greeting + ""</h1>"";
            }
        };

        var foo = new Foo(""Hi Foo"");
        document.body.innerHTML = foo.greet();

        var bar = new Bar(""Hello, world!"");
        document.body.innerHTML = bar.greet();";

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseNetPack();
            app.UseStaticFiles(new StaticFileOptions() { });
          
            app.Run(async (a) =>
            {
                var changeFileKey = "Change";
                if (a.Request.Query.ContainsKey(changeFileKey))
                {
                    StringValues values;
                    if (a.Request.Query.TryGetValue(changeFileKey, out values))
                    {
                        foreach (var value in values)
                        {
                            var subPath = SubPathInfo.Parse(value);
                            
                            var existingFile = InMemoryFileProvider.GetFileInfo(value);
                            var existingFileContents = existingFile.ReadAllContent();
                            var modifiedFileContents = existingFileContents + Environment.NewLine +
                                                       "// modified on " + DateTime.UtcNow;
                            
                            var retrievedFolder = InMemoryFileProvider.Directory.GetFolder(subPath.Directory);
                            var modifiedFile = new StringFileInfo(modifiedFileContents, subPath.Name);
                            retrievedFolder.UpdateFile(modifiedFile);

                        }
                    }

                    await a.Response.WriteAsync("done");
                }


            });

        }

        public InMemoryFileProvider InMemoryFileProvider { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {

            var inputFileProvider = new InMemoryFileProvider();
            inputFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(TsContentOne, "somefile.ts"));
            inputFileProvider.Directory.AddFile("incremental", new StringFileInfo(TsContentFoo, "foo.ts"));
            inputFileProvider.Directory.AddFile("incremental", new StringFileInfo(TsContentBar, "bar.ts"));
            InMemoryFileProvider = inputFileProvider;

            services.AddNetPack((setup) =>
            {
                setup.AddPipeline(a =>
                {
                    a.WithFileProvider(inputFileProvider)
                  // Simple processor, that compiles typescript files.
                  .AddTypeScriptPipe(input =>
                  {
                      input.Include("wwwroot/*.ts");
                  }, options =>
                  {
                      // configure various typescript compilation options here
                      options.Module = ModuleKind.AMD;
                      options.SourceMap = true;
                      // options.InlineSourceMap = true;
                      //  options.Module = ModuleKind.Amd;
                  })
                  .AddTypeScriptPipe(input =>
                  {
                      input.Include("incremental/*.ts");
                  }, options =>
                  {
                      // configure various typescript compilation options here
                      options.Module = ModuleKind.AMD;
                      options.SourceMap = true;
                      options.OutFile = "combined.js";
                      // options.InlineSourceMap = true;
                      //  options.Module = ModuleKind.Amd;
                  })
                  .UseBaseRequestPath("/netpack")

                  // allows the files produced from processing to be resolved via the environemtns webroot file provider..
                  .Watch(); // Input files are watched, and when changes occur, pipeline will automatically trigger necessary processing.

                });

            });
        }
    }
}