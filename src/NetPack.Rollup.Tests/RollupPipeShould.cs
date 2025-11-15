using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dazinator.Extensions.FileProviders;
using Dazinator.Extensions.FileProviders.InMemory;
using NetPack.Tests.Utils;
using Xunit;
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace NetPack.Rollup.Tests
{
    public class RollupPipeShould
    {

        private readonly TestServer _server;
        private readonly HttpClient _client;

        public RollupPipeShould()
        {
            NodeFnmHelper.SetPath();
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<RollupPipeShouldTestsStartup>());
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
        public async Task Create_Bundle_File_With_SourceMap()
        {
            // Act
            string responseString = await GetResponseString();
            Assert.Contains("File: built/bundle.js", responseString);
            Assert.Contains("File: built/bundle.js.map", responseString);
        }

        [Fact]
        public async Task Create_Iife_Bundle_With_Variable_Name()
        {
            // Act
            string responseString = await GetResponseString();
            Assert.Contains("File: built/iifebundle.js", responseString);
            Assert.Contains("File: built/iifebundle.js.map", responseString);
            Assert.Contains("var mybundle = (function (exports) {", responseString);
        }

        [Fact]
        public async Task Create_Bundle_With_External_Modules()
        {
            // Act
            string responseString = await GetResponseString("path=external");
            Assert.Contains("File: built/bundlewithexternal.js", responseString);
            Assert.Contains("File: built/bundlewithexternal.js.map", responseString);
            Assert.Contains("System.register(['SomeExternalLib'], function (exports, module) {", responseString);
        }

        [Fact]
        public async Task Create_Bundle_With_External_Globals()
        {
            // Act
            string responseString = await GetResponseString("path=external");
            Assert.Contains("File: built/bundlewithexternal.js", responseString);
            Assert.Contains("File: built/bundlewithexternal.js.map", responseString);
            Assert.Contains("var mybundlewithglobal = (function (exports,jjj) {", responseString);
        }

        [Fact]
        public async Task Create_Bundle_With_Amd_Id()
        {
            // Act
            string responseString = await GetResponseString("path=externalcdn");
            Assert.Contains("File: built/bundlewithexternalglobalcdn.js", responseString);
            Assert.Contains("File: built/bundlewithexternalglobalcdn.js.map", responseString);
            Assert.Contains("define('custom'", responseString);
        }

        [Fact]
        public async Task Create_Bundle_With_External_Module_From_CDN()
        {
            // Act
            string responseString = await GetResponseString("path=externalcdn");
            Assert.Contains("File: built/bundlewithexternalglobalcdn.js", responseString);
            Assert.Contains("File: built/bundlewithexternalglobalcdn.js.map", responseString);
            Assert.Contains("https://cdnjs.cloudflare.com/ajax/libs/jquery/3.3.1/jquery.min.js", responseString);
        }

        [Fact]
        public async Task Create_Multiple_Format_Bundles_From_Same_Inputs()
        {
            // Act
            string responseString = await GetResponseString();
            Assert.Contains("File: built/modulesbundle.js", responseString);
            Assert.Contains("File: built/nomodulesbundle.js", responseString);
        }

        public class RollupPipeShouldTestsStartup
        {
            public const string AmdModuleAFileContent = @"
export class ClassA {
    constructor(another) { }
    doSomething() {
        // return ""<h1>"" + this.greeting + ""</h1>"";
    }
}

var classA = new ClassA(""Hello, world!"");
classA.doSomething();

";

            public const string AmdModuleBFileContent = @"

      import {ClassA} from ""./ModuleA"";


export class ClassB
        {
            constructor(another) { }
            doSomething()
            {
                // return ""<h1>"" + this.greeting + ""</h1>"";
            }
        };

        var classB = new ClassB(""Hello, world!"");
        classB.doSomething();

var classA = new ClassA(""Hello, world!"");
        classA.doSomething();

";

            public const string ModuleWithExternalDependency = @"

import {foo} from ""SomeExternalLib"";


export class ClassA {
    constructor(another) { }
    doSomething() {
    foo.bar();
        // return ""<h1>"" + this.greeting + ""</h1>"";
    }
}

var classA = new ClassA(""Hello, world!"");
classA.doSomething();

";

            public const string ModuleWithExternalGlobalDependency = @"

import jjj from 'jquery';


export class ClassA {
    constructor(another) { }
    doSomething() {
    jjj.bar();
        // return ""<h1>"" + this.greeting + ""</h1>"";
    }
}

var classA = new ClassA(""Hello, world!"");
classA.doSomething();

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
                InMemoryFileProvider inMemoryFileProvider = new InMemoryFileProvider();
                inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(AmdModuleAFileContent, "ModuleA.js"));
                inMemoryFileProvider.Directory.AddFile("wwwroot", new StringFileInfo(AmdModuleBFileContent, "ModuleB.js"));
                inMemoryFileProvider.Directory.AddFile("wwwroot/external", new StringFileInfo(ModuleWithExternalDependency, "ModuleWithExternalDependency.js"));
                inMemoryFileProvider.Directory.AddFile("wwwroot/external", new StringFileInfo(ModuleWithExternalGlobalDependency, "ModuleWithExternalGlobalDependency.js"));

                services.AddNetPack((setup) =>
                {

                    NetPackServicesExtensions.FileProcessingOptions fileProcessingBuilder = setup.AddPipeline(a =>
                    {

                        a.WithFileProvider(inMemoryFileProvider)
                            .AddRollupPipe(input =>
                            {
                                input.Include("wwwroot/*.js");
                            }, options =>
                            {
                                options.InputOptions.Input = "/wwwroot/ModuleB.js";
                                options.HasOutput((output) =>
                                {
                                    output.Format = Rollup.RollupOutputFormat.Esm;
                                    output.File = "bundle.js";
                                    output.Sourcemap = SourceMapType.File;
                                });
                               
                            })
                             .AddRollupPipe(input =>
                             {
                                 input.Include("wwwroot/*.js");
                             }, options =>
                             {                               

                                 options.InputOptions.Input = "/wwwroot/ModuleB.js";
                                 options.HasOutput((output) =>
                                 {
                                     output.Format = Rollup.RollupOutputFormat.Iife;
                                     output.File = "iifebundle.js";
                                     output.Sourcemap = SourceMapType.Inline;
                                     output.Name = "mybundle";
                                 });
                                 
                             })
                              .AddRollupPipe(input =>
                              {
                                  input.Include("wwwroot/external/ModuleWithExternalDependency.js");
                              }, options =>
                              {
                                  options.InputOptions.Input = "/wwwroot/external/ModuleWithExternalDependency.js";
                                  options.InputOptions.External.Add("SomeExternalLib");

                                  options.HasOutput((output) =>
                                  {
                                      output.Format = Rollup.RollupOutputFormat.System;
                                      output.File = "/external/bundlewithexternal.js";
                                      output.Sourcemap = SourceMapType.File;
                                  });                                  
                                  //options.OutputOptions.Sourcemap = SourceMapType.File;
                                  //options.OutputOptions.Name = "mybundle";
                              })
                               .AddRollupPipe(input =>
                               {
                                   input.Include("wwwroot/external/ModuleWithExternalGlobalDependency.js");
                               }, options =>
                               {
                                   options.InputOptions.Input = "/wwwroot/external/ModuleWithExternalGlobalDependency.js";
                                   options.InputOptions.External.Add("jjj");
                                   options.HasOutput((output) =>
                                   {
                                       output.Format = Rollup.RollupOutputFormat.Iife;
                                       output.File = "/external/bundlewithexternalglobal.js";
                                       output.Name = "mybundlewithglobal";
                                       output.Sourcemap = SourceMapType.File;
                                       output.ConfigureGlobals(globals =>
                                       {
                                           globals["jquery"] = JsonValue.Create("jjj");
                                       });
                                       output.ConfigurePaths(paths =>
                                       {
                                           //  configure path for jquery module to be loaded from CDN.
                                           paths["jquery"] = JsonValue.Create("https://cdnjs.cloudflare.com/ajax/libs/jquery/3.3.1/jquery.min.js");
                                       });
                                   });

                                   

                                   //options.OutputOptions.Sourcemap = SourceMapType.File;
                                   //options.OutputOptions.Name = "mybundle";
                               })
                                .AddRollupPipe(input =>
                                {
                                    input.Include("wwwroot/external/ModuleWithExternalGlobalDependency.js");
                                }, options =>
                                {
                                    options.InputOptions.Input = "/wwwroot/external/ModuleWithExternalGlobalDependency.js";
                                    options.InputOptions.External.Add("jjj");

                                    options.HasOutput((output) =>
                                    {
                                        output.Format = Rollup.RollupOutputFormat.Amd;
                                        output.File = "/externalcdn/bundlewithexternalglobalcdn.js";
                                        output.Name = "mybundlewithglobalcdn";
                                        output.Sourcemap = SourceMapType.File;
                                        output.ConfigureGlobals(globals =>
                                        {
                                            globals["jquery"] = JsonValue.Create("jjj");
                                        });
                                        output.ConfigurePaths(paths =>
                                        {
                                            //  configure path for jquery module to be loaded from CDN.
                                            paths["jquery"] = JsonValue.Create("https://cdnjs.cloudflare.com/ajax/libs/jquery/3.3.1/jquery.min.js");
                                        });
                                        output.ConfigureAmd(amd =>
                                        {
                                            amd["id"] = JsonValue.Create("custom");
                                        });
                                    });
                                    //options.OutputOptions.Sourcemap = SourceMapType.File;
                                    //options.OutputOptions.Name = "mybundle";
                                })
                                .AddRollupPipe(input =>
                                {
                                    input.Include("wwwroot/*.js");
                                }, options =>
                                {
                                    options.InputOptions.Input = "/wwwroot/ModuleB.js";
                                    options.HasOutput((output) =>
                                    {
                                        output.Format = Rollup.RollupOutputFormat.Esm;
                                        output.File = "modulesbundle.js";
                                        output.Sourcemap = SourceMapType.File;
                                    });
                                    options.HasOutput((output) =>
                                    {
                                        output.Format = Rollup.RollupOutputFormat.System;
                                        output.File = "nomodulesbundle.js";
                                        output.Sourcemap = SourceMapType.File;
                                    });

                                })


                            .UseBaseRequestPath("/built");
                    });

                });
            }
        }
    }
}