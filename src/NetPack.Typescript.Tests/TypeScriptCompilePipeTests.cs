using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using Moq;
using NetPack.Requirements;
using NetPack.Utils;
using Xunit;
using Dazinator.AspNet.Extensions.FileProviders;
using NetPack.Typescript;
using NetPack.Tests.Pipes;

namespace NetPack.Typescript.Tests
{
    public class TypeScriptCompilePipeTests : PipeTestBase
    {

        [Fact]
        public async Task Compiles_TypescriptFiles_Into_Js_Files_With_SourceMaps()
        {

            // arrange
            var mockNodeInstance = new Moq.Mock<INetPackNodeServices>();
            mockNodeInstance.Setup(a => a.InvokeExportAsync<TypeScriptCompileResult>(It.IsAny<string>(),"build", It.IsAny<TypescriptCompileRequestDto>()))
                                         .ReturnsAsync(new TypeScriptCompileResult()
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
            ThenTheProcessedOutputDirectoryFile("SomeFolder/somefile.js", Assert.NotNull);
            ThenTheProcessedOutputDirectoryFile("SomeFolder/somefile.js.map", Assert.NotNull);

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