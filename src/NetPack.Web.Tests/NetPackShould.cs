using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dazinator.Extensions.FileProviders;
using Dazinator.Extensions.FileProviders.InMemory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NetPack.Tests.Utils;
using Xunit;
using NetPack.Typescript;
using NetPack.Utils;

namespace NetPack.Web.Tests
{
    public class NetPackShould
    {

        private readonly TestServer _server;
        private readonly HttpClient _client;

        public NetPackShould()
        {
            NodeFnmHelper.SetPath();
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()
                .UseContentRoot(Environment.CurrentDirectory));
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task Serve_Files_Output_From_Pipeline()
        {
            // This test shows that when we request a .js file, we get one, even though
            // it doesn't physically exist on disk, it has been produced in memory
            // as a result of the netpack pipeline processing typescript files
            // in our application, configured in the applications startup.cs

            // Act
            var responseString = await GetResponseString("/netpack/ts/somefile.js");

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(responseString));

        }

        [Fact]
        public async Task Serve_Updated_Outputs_When_Inputs_Change()
        {
            // This test shows that:

            //1. We request a javascript file, that was output from our pipeline that compiles typescript into javascript.
            //2. We trigger the changeToken for the typescript file on the server, mimicking the typescript file being edited 
            // with the phrase "// modified on" and the current datetime being appended to the original contents.
            //3. We allow the pipeline time to detect the modified file, and re-process.
            //4. We request the same javascript file again, but this time, we get back the updated contents, showing that
            //   as we change files, our pipeline is updating outputs for us automatically, 
            //   and these are then being served to the browser when requested.

            //1.
            var originalFileContents = await GetResponseString("netpack/ts/somefile.js");

            //2. This causes the `IChangeToken for the file `wwwroot/somefile.ts` on the server to activate,
            // and as the pipeline is watching its inputs, it should detect this and re-process.
            await GetResponseString("/", "change=ts/somefile.ts");

            //3. The pipeline should re-process all its inputs to produce updated outputs.
            // Give it 5 seconds to complete this.
            await Task.Delay(new TimeSpan(0, 0, 5));

            //4. Now request the same javascript file again - it should have been autoamtically updated by the pipeline.
            var updatedFileContents = await GetResponseString("netpack/ts/somefile.js");

            Assert.NotEqual(originalFileContents, updatedFileContents);
            Assert.Contains("// modified on", updatedFileContents);


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

            public const string TsContentTwo = @"

        class Another
        {
    constructor(public another: string) { }
    doSomething()
            {
               // return ""<h1>"" + this.greeting + ""</h1>"";
            }
        };

        var another = new Another(""Hello, world!"");
        another.doSomething();";

            public void ConfigureServices(IServiceCollection services)
            {

                var mockFileProvider = new InMemoryFileProvider();

                mockFileProvider.Directory.AddFile("ts", new StringFileInfo(TsContentOne, "somefile.ts"));
                mockFileProvider.Directory.AddFile("ts", new StringFileInfo(TsContentTwo, "someOtherfile.ts"));

                InMemoryFileProvider = mockFileProvider;

                services.AddNetPack((setup) =>
                {
                    setup.AddPipeline(pipelineBuilder =>
                    {
                        pipelineBuilder.WithFileProvider(mockFileProvider)
                        .AddTypeScriptPipe(input =>
                        {
                            input.Include("ts/somefile.ts")
                                 .Include("ts/someOtherfile.ts");
                        }, tsConfig =>
                        {
                            tsConfig.Target = ScriptTarget.ES5;
                            tsConfig.Module = ModuleKind.AMD;
                            tsConfig.NoImplicitAny = true;
                            tsConfig.RemoveComments = false;
                            // important: we are not removing comments because we test for a modification that involves a comment being added!
                            tsConfig.SourceMap = true;
                        }
                        )
                        .UseBaseRequestPath("/netpack")
                        .Watch();


                    });
                });
            }

            public InMemoryFileProvider InMemoryFileProvider { get; set; }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {

               

                // env.WebRootFileProvider = mockFileProvider;
                //   env.WebRootFileProvider = 


                app.UseNetPack();

                app.UseStaticFiles();

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

                    if (a.Request.Path.Value.Contains("netpack/ts/somefile.js"))
                    {
                        var file = env.WebRootFileProvider.GetFileInfo("/netpack/ts/somefile.js");
                        
                       // await SendFileResponseExtensions.SendFileAsync(a.Response, file);
                    }
                });

               
            }


        }

    }
}