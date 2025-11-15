using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dazinator.Extensions.FileProviders;
using Dazinator.Extensions.FileProviders.InMemory;
using NetPack.Tests.Utils;
using Xunit;

namespace NetPack.Rollup.Tests
{
    public class RollupCodeSplittingPipeShould
    {

        private readonly TestServer _server;
        private readonly HttpClient _client;

        public RollupCodeSplittingPipeShould()
        {
            NodeFnmHelper.SetPath();
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<RollupCodeSplittingPipeShouldTestsStartup>());
            _client = _server.CreateClient();
        }

        private async Task<string> GetResponseString(string querystring = "")
        {
            string request = "/run";
            if (!string.IsNullOrEmpty(querystring))
            {
                request += "?" + querystring;
            }

            HttpResponseMessage response = await _client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async Task Create_Code_Split_Bundle()
        {
            // Act
            string responseString = await GetResponseString("path=rollup");
            Assert.Contains("File: built/Main.js", responseString);
            Assert.Contains("File: built/Main.js.map", responseString);
            Assert.Contains("File: built/Second.js", responseString);
            Assert.Contains("File: built/Second.js.map", responseString);
        }

        [Fact]
        public async Task Create_Multiple_Format_Bundles_From_Same_Inputs()
        {
            // Act
            string responseString = await GetResponseString("path=rollup/modules");
            Assert.Contains("File: built/Main.js", responseString);
            Assert.Contains("File: built/Main.js.map", responseString);
            Assert.Contains("File: built/Second.js", responseString);
            Assert.Contains("File: built/Second.js.map", responseString);

            responseString = await GetResponseString("path=rollup/nomodules");
            Assert.Contains("File: built/Main.js", responseString);
            Assert.Contains("File: built/Main.js.map", responseString);
            Assert.Contains("File: built/Second.js", responseString);
            Assert.Contains("File: built/Second.js.map", responseString);
        }

    }

    public class RollupCodeSplittingPipeShouldTestsStartup
    {
        public const string MainJs = @"
export default function () {
  console.log('main');
}
";

        public const string SecondJs = @"

     export default function () {
  return import('./Main.js').then(({ default: main }) => {
    main();
  });
}

";
        //public const string ConfigFileContent =
        //    @"requirejs.config({\r\n baseUrl: \'wwwroot\',\r\n    paths: {\r\n        app: \'..\/app\'\r\n    }\r\n});";

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            app.UseNetPack();

            app.Run(async (context) =>
            {

                StringValues? path = context.Request.Query?["path"];
                string pathText = path.GetValueOrDefault().ToString();


                // Write the content of outputs files to the response for inspection.
                StringBuilder builder = new StringBuilder();

                string outputPath = "built";
                if (!string.IsNullOrWhiteSpace(pathText))
                {
                    outputPath = outputPath + "/" + pathText;
                }

                foreach (Microsoft.Extensions.FileProviders.IFileInfo outputFile in env.WebRootFileProvider.GetDirectoryContents(outputPath))
                {
                    if (!outputFile.IsDirectory)
                    {
                        using (StreamReader reader = new StreamReader(outputFile.CreateReadStream()))
                        {
                            builder.AppendLine("File: " + "built" + "/" + outputFile.Name);
                            builder.Append(reader.ReadToEnd());
                            builder.AppendLine();
                        }
                    }
                }

                await context.Response.WriteAsync(builder.ToString());
            });
        }
        public void ConfigureServices(IServiceCollection services)
        {

            // Provide some in memory files, that are AMD modules to be used as input for the RequireJs Optimise Pipe
            var inMemoryFileProvider = new InMemoryFileProvider();
            inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(MainJs, "Main.js"));
            inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(SecondJs, "Second.js"));

            services.AddNetPack((setup) =>
            {

                var fileProcessingBuilder = setup.AddPipeline(a =>
                {

                    a.WithFileProvider(inMemoryFileProvider)
                        .AddRollupCodeSplittingPipe(input =>
                        {
                            input.Include("wwwroot/*.js");
                        }, options =>
                        {
                            options.InputOptions.AddEntryPoint("/wwwroot/Main.js")
                                                .AddEntryPoint("/wwwroot/Second.js");

                            options.HasOutput((output) => {
                                output.Format = Rollup.RollupOutputFormat.Esm;
                                output.Sourcemap = SourceMapType.File;
                                output.Dir = "/rollup";                                
                            });
                           
                        })
                        // Example of a rollup build that produces multiple output formats from same set of inputs.
                         .AddRollupCodeSplittingPipe(input =>
                         {
                             input.Include("wwwroot/*.js");
                         }, options =>
                         {
                             options.InputOptions.AddEntryPoint("/wwwroot/Main.js")
                                                 .AddEntryPoint("/wwwroot/Second.js");

                             options.HasOutput((output) => {
                                 output.Format = Rollup.RollupOutputFormat.Esm;
                                 output.Sourcemap = SourceMapType.File;
                                 output.Dir = "/rollup/modules";
                             });

                             options.HasOutput((output) => {
                                 output.Format = Rollup.RollupOutputFormat.System;
                                 output.Sourcemap = SourceMapType.File;
                                 output.Dir = "/rollup/nomodules";
                             });

                         })
                        .UseBaseRequestPath("/built");
                });

            });
        }
    }
}