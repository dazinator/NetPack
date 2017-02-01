using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NetPack.Pipeline;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace NetPack.Tests
{
    public class RequestHaltingMiddlewareTests
    {

        [Fact]
        public async Task Delays_Request_Whilst_File_Locked()
        {

            var inMemoryFileProvider = new InMemoryFileProvider();
            inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo("hi", "foo.ts"));

            bool skipDelay = true;

            var builder = new WebHostBuilder()
                .UseContentRoot(Environment.CurrentDirectory)
                .Configure(app =>
                {
                    app.UseFileProcessing();
                    app.UseStaticFiles();
                })
                .ConfigureServices((services) =>
                {
                    services.AddNetPack((setup) =>
                    {
                        setup.AddFileProcessing(options =>
                        {
                            options.WithFileProvider(inMemoryFileProvider)
                                .AddPipe(inputs => inputs.Input.AddInclude("wwwroot/foo.ts"), new DelegatePipe(async (context, token) =>
                                {
                                    // block requests for the file we are generating, until we have finished generating it.
                                    var generatedFilePath = "wwwroot/foo.js";
                                    using (FileRequestServices.BlockFilePath(generatedFilePath))
                                    {
                                        // simulate some work
                                        if (!skipDelay)
                                        {
                                            await Task.Delay(new TimeSpan(0, 0, 10));
                                        }

                                        var inputFileContents = context.InputFiles[0].FileInfo.ReadAllContent();
                                        // output the generated file
                                        context.PipelineContext.AddGeneratedOutput("wwwroot", new StringFileInfo(inputFileContents + " processed!", "foo.js"));
                                    } // lock freed on dispose, should allow any in progress request for file to continue.
                                }))
                                .Watch();
                        });
                    });
                });



            var server = new TestServer(builder);

            // make a request for the generated file. This will allow everythign to warm up.
            var response = await server.CreateClient().GetAsync("wwwroot/foo.js");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var contents = await response.Content.ReadAsStringAsync();
            Assert.NotNull(contents);
            Assert.Equal("hi processed!", contents);

            // Now, change an input file. As Watch() is specified, this will cause the file processing pipeline to re-execute, and generate anew output file. 
            // Whilst the new generated file is being generated, requests for the generated file should be delayed.
            skipDelay = false;
            inMemoryFileProvider.Directory.AddOrUpdateFile("wwwroot", new StringFileInfo("changed!", "foo.ts"));
            
            // allow a small delay before we send the request, as it can be upto 2 seconds between file watching detecting the change and the fiel processing pipeline being re-extectued.
          //  await Task.Delay(new TimeSpan(0, 0, 2));
            response = await server.CreateClient().GetAsync("wwwroot/foo.js");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            contents = await response.Content.ReadAsStringAsync();
            Assert.NotNull(contents);
            Assert.Equal("changed! processed!", contents);
            Assert.False(FileRequestServices.HasLocks());

        }


    }
}
