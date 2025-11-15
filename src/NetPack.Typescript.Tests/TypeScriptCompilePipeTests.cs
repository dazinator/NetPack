using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using NetPack.Requirements;
using NetPack.Utils;
using Xunit;
using NetPack.Tests.Pipes;
using System.Threading;
using Dazinator.Extensions.FileProviders;
using NetPack.Tests.Utils;

namespace NetPack.Typescript.Tests
{
    public class TypeScriptCompilePipeTests : PipeTestBase
    {
        public TypeScriptCompilePipeTests()
        {
            NodeFnmHelper.SetPath();
        }

        [Fact]
        public async Task Compiles_TypescriptFiles_Into_Js_Files_With_SourceMaps()
        {

            // arrange
            var mockNodeInstance = new Moq.Mock<INetPackNodeServices>();
            mockNodeInstance.Setup(a => a.InvokeExportAsync<TypescriptCompileRequestDto, TypeScriptCompileResult>(It.IsAny<StringAsTempFile>(),"build", It.IsAny<TypescriptCompileRequestDto>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(new TypeScriptCompileResult()
                                         {
                                             Sources = new Dictionary<string, string>()
                                             {
                                                 { "SomeFolder/somefile.js", "some code" },
                                                 { "SomeFolder/somefile.js.map", "some map code" }
                                             }
                                         });

          //  mockNodeInstance.Setup(a => a.CreateStringAsTempFile(It.IsAny<string>())).Returns(new StringAsTempFile("blah", CancellationToken.None));

            var mockJsRequirement = new Moq.Mock<NodeJsIsInstalledRequirement>();
            mockJsRequirement.Setup(a => a.Check(null));

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