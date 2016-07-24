using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Pipeline;
using NetPack.Pipes;
using NetPack.Requirements;
using Xunit;
using NetPack.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;

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
        public async void Serve_Files_Output_From_Pipeline()
        {

            // Act
            var responseString = await GetResponseString("wwwroot/somefile.js");
            Assert.False(string.IsNullOrWhiteSpace(responseString));
            // Assert
            //   Assert.Equal("Pass in a number to check in the form /checkprime?5", responseString);

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

                app.UseNetPackPipeLine(pipelineBuilder =>
                {
                    var pipeLine = pipelineBuilder.AddTypeScriptPipe(tsConfig =>
                    {
                        tsConfig.Target = TypeScriptPipeOptions.ScriptTarget.Es5;
                        tsConfig.Module = TypeScriptPipeOptions.ModuleKind.CommonJs;
                        tsConfig.NoImplicitAny = true;
                        tsConfig.RemoveComments = true;
                        tsConfig.SourceMap = true;
                    })
                        //.AddPipe(someOtherPipe)
                        .WithInput((inputBuilder) =>
                            inputBuilder.Include("wwwroot/somefile.ts")
                                        .Include("wwwroot/someOtherfile.ts")
                                        .Input)
                        .PipeLine;

                    return pipeLine;

                });

                app.UseStaticFiles();


                //app.Run(async (context) =>
                //{

                //    var path = context.Request.Path.ToString();
                //    await context.Response.WriteAsync("<div>Inside middleware defined using app.Run</div>");

                //});

            }
        }

    }
}