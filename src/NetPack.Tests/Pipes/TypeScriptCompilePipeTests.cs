using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using Moq;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Pipes;
using NetPack.Pipes.Typescript;
using NetPack.Requirements;
using NetPack.Utils;
using Xunit;
using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders;
using System;

namespace NetPack.Tests.Pipes
{
    public class TypeScriptCompilePipeTests : PipeTestBase
    {

        [Fact]
        public async Task Compiles_TypescriptFiles_Into_Js_Files_With_SourceMaps()
        {

            // arrange
            var mockNodeInstance = new Moq.Mock<INodeServices>();
            mockNodeInstance.Setup(a => a.InvokeAsync<TypeScriptCompilePipe.TypeScriptCompileResult>(It.IsAny<string>(), It.IsAny<TypeScriptCompilePipe.TypescriptCompileRequestDto>()))
                                         .ReturnsAsync(new TypeScriptCompilePipe.TypeScriptCompileResult()
                                         {
                                             Sources = new Dictionary<string, string>()
                                             {
                                                 { "SomeFolder/somefile.js", "some code" },
                                                 { "SomeFolder/somefile.js.map", "some map code" }
                                             }
                                         });

            var mockJsRequirement = new Moq.Mock<NodeJsRequirement>();
            mockJsRequirement.Setup(a => a.Check());

            var embeddedScript = new StringFileInfo("some embedded script", "netpack-typescript");
            var mockEmbeddedResources = new Moq.Mock<IEmbeddedResourceProvider>();
            mockEmbeddedResources.Setup(a => a.GetResourceFile(It.IsAny<Assembly>(), It.IsAny<string>())).Returns(embeddedScript);

            // Act
            var typescriptFileOne = GivenAFileInfo("SomeFolder/somefile.ts", () => TsContentOne);

            // When file processed by the typescript compile pipe
            await WhenFilesProcessedByPipe(() =>
            {
                var options = new TypeScriptPipeOptions();
                return new TypeScriptCompilePipe(mockNodeInstance.Object, mockEmbeddedResources.Object, options);
            }, typescriptFileOne);


            // Assert
            ThenTheOutputFileFromPipe("SomeFolder/somefile.js", Assert.NotNull);
            ThenTheOutputFileFromPipe("SomeFolder/somefile.js.map", Assert.NotNull);

        }


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


    }
}