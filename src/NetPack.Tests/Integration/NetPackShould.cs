using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipes;
using Xunit;
using NetPack.Extensions;

namespace NetPack.Tests.Integration
{
    public class NetPackShould
    {


        private readonly TestServer _server;
        private readonly HttpClient _client;

        public NetPackShould()
        {
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()
                .UseContentRoot(Environment.CurrentDirectory));
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
        public async void Serve_Files_Output_From_Pipeline()
        {
            // This test shows that when we request a .js file, we get one, even though
            // it doesn't physically exist on disk, it has been produced in memory
            // as a result of the netpack pipeline processing typescript files
            // which is configured in the applications startup.cs
            // Act
            var responseString = await GetResponseString("wwwroot/somefile.js");

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(responseString));

        }

        public class Startup
        {

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddNetPack();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {

                var mockFileProvider = TestUtils.GetMockFileProvider(new[] { "wwwroot/somefile.ts", "wwwroot/someOtherfile.ts" }, new[] { TestUtils.TsContentOne, TestUtils.TsContentTwo });
                env.ContentRootFileProvider = mockFileProvider;

                app.UseContentPipeLine(pipelineBuilder =>
                {
                    return pipelineBuilder
                        //.AddPipe(someOtherPipe)
                        .WithInput((inputBuilder) 
                                     => inputBuilder
                                        .Include("wwwroot/somefile.ts")
                                        .Include("wwwroot/someOtherfile.ts"))
                                        .WatchInputForChanges()
                        .DefinePipeline()
                            .AddTypeScriptPipe(tsConfig =>
                                     {
                                         tsConfig.Target = TypeScriptPipeOptions.ScriptTarget.Es5;
                                         tsConfig.Module = TypeScriptPipeOptions.ModuleKind.CommonJs;
                                         tsConfig.NoImplicitAny = true;
                                         tsConfig.RemoveComments = true;
                                         tsConfig.SourceMap = true;
                                     })
                        .BuildPipeLine();
                })
                .UsePipelineOutputAsStaticFiles();
            }
        }

    }
}