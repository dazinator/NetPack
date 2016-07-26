using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Moq;
using NetPack.Pipes;
using Xunit;
using NetPack.Extensions;
using NetPack.File;
using NetPack.Pipeline;

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
            // in our application, configured in the applications startup.cs

            // Act
            var responseString = await GetResponseString("wwwroot/somefile.js");

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(responseString));

        }

        [Fact]
        public async void Serve_Updated_Outputs_When_Inputs_Change()
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
            var originalFileContents = await GetResponseString("wwwroot/somefile.js");

            //2. This causes the `IChangeToken for the file `wwwroot/somefile.ts` on the server to activate,
            // and as the pipeline is watching its inputs, it should detect this and re-process.
            await GetResponseString("/", "change=wwwroot/somefile.ts");

            //3. The pipeline should re-process all its inputs to produce updated outputs.
            // Give it 5 seconds to complete this.
            await Task.Delay(new TimeSpan(0, 0, 5));

            //4. Now request the same javascript file again - it should have been autoamtically updated by the pipeline.
            var updatedFileContents = await GetResponseString("wwwroot/somefile.js");

            Assert.NotEqual(originalFileContents, updatedFileContents);
            Assert.True(updatedFileContents.Contains("// modified on"));


        }

        public class Startup
        {

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddNetPack();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {


                var mockDisposable = new Moq.Mock<IDisposable>();
                var callBacks = new Dictionary<string, List<Action<object>>>();
                var changeTokens = new Dictionary<string, Moq.Mock<IChangeToken>>();

                var mockFileProvider = TestUtils.GetMockFileProvider(new[] { "wwwroot/somefile.ts", "wwwroot/someOtherfile.ts" }, new[] { TestUtils.TsContentOne, TestUtils.TsContentTwo },
                    filePath =>
                    {
                        var mockChangeToken = new Moq.Mock<IChangeToken>();
                        mockChangeToken.SetupAllProperties();
                        mockChangeToken.Setup(a => a.RegisterChangeCallback(It.IsAny<Action<Object>>(), It.IsAny<object>()))
                   .Returns<Action<object>, object>((callback, b) =>
                   {
                       if (!callBacks.ContainsKey(filePath))
                       {
                           callBacks[filePath] = new List<Action<object>>();
                       }
                       callBacks[filePath].Add(callback);
                       return mockDisposable.Object;
                   });

                        changeTokens.Add(filePath, mockChangeToken);
                        return mockChangeToken.Object;
                    });

                // quick way to override some stuff when i need to..
                var wrappedFileProvider = new TestFileProvider(mockFileProvider);
                env.ContentRootFileProvider = wrappedFileProvider;

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


                                var changeToken = changeTokens[value];
                                changeToken.SetupGet(x => x.HasChanged).Returns(true);

                                string fileName = value;
                                var lastIndex = value.LastIndexOf("/");
                                if (lastIndex > -1)
                                {
                                    fileName = value.Substring(lastIndex + 1);
                                }

                                var existingFile = wrappedFileProvider.GetFileInfo(value);
                                using (var reader = new StreamReader(existingFile.CreateReadStream()))
                                {
                                    var builder = new StringBuilder();
                                    var existingFileContents = reader.ReadToEnd();
                                    builder.Append(existingFileContents);
                                    builder.AppendLine();
                                    builder.AppendLine(" // modified on " + DateTime.UtcNow);
                                    var modifiedContents = builder.ToString();

                                    var changedFileInfo = new StringFileInfo(modifiedContents, fileName);
                                    wrappedFileProvider.Files[value] = changedFileInfo;

                                    var onChangedCallbacks = callBacks[value];
                                    foreach (var callback in onChangedCallbacks)
                                    {
                                        // trigerring the change callbacks for the token, 
                                        // all subscribers will be notified that this file has changed.
                                        callback.Invoke(changedFileInfo);
                                    }
                                }





                            }
                        }

                        await a.Response.WriteAsync("done");
                    }
                });
            }
        }

    }
}