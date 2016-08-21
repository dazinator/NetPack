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

namespace NetPack.Tests.Pipes
{
    public class TypeScriptCompilePipeTests
    {

        [Fact]
        public async Task Processes_TypescriptFiles_And_Outputs_Js_Files()
        {

            // arrange
            var mockNodeInstance = new Moq.Mock<INodeServices>();
            mockNodeInstance.Setup(a => a.InvokeAsync<TypeScriptCompilePipe.TypeScriptCompileResult>(It.IsAny<string>(), It.IsAny<TypeScriptCompilePipe.TypescriptCompileRequestDto>()))
                                .ReturnsAsync(new TypeScriptCompilePipe.TypeScriptCompileResult() { Sources = new Dictionary<string, string>() { { "SomeFolder/somefile.js", "come code"} } });
                               //.ReturnsAsync(new TypeScriptCompileResult() { });

            var mockJsRequirement = new Moq.Mock<NodeJsRequirement>();
            mockJsRequirement.Setup(a => a.Check());

            var testInputFiles = new SourceFile[]
            {
                new SourceFile(new StringFileInfo(TsContentOne, "somefile.ts"), "SomeFolder")
            };

            var embeddedScript = new StringFileInfo("some embedded script", "netpack-typescript");

            var mockEmbeddedResources = new Moq.Mock<IEmbeddedResourceProvider>();
            mockEmbeddedResources.Setup(a => a.GetResourceFile(It.IsAny<Assembly>(), It.IsAny<string>())).Returns(embeddedScript);

            IPipe sut = new TypeScriptCompilePipe(mockNodeInstance.Object, mockEmbeddedResources.Object);

            var pipelineContext = new Moq.Mock<IPipelineContext>();
            pipelineContext.Setup(a => a.Input).Returns(testInputFiles);

            var outputFiles = new List<SourceFile>();
            pipelineContext.Setup(a => a.AddOutput(It.IsAny<SourceFile>())).Callback<SourceFile>((a) =>
            {
                outputFiles.Add(a);
            });

            
            // act
            await sut.ProcessAsync(pipelineContext.Object, CancellationToken.None);

            // assert

            // should output the compiled typescript file and the source ts file cos source maps enabled
            Assert.Equal(outputFiles.Count, 2);
            var outputFile = outputFiles[1];

            // should name the output file .js not .ts.
            Assert.Equal(outputFile.FileInfo.Name, "somefile.js");

            // The output should have a directory returned from node.
            Assert.Equal(outputFile.ContentPathInfo.Directory.ToString(), "SomeFolder");

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