using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.FileProviders;
using Moq;
using NetPack.Pipeline;
using NetPack.Pipes;
using NetPack.Requirements;
using Xunit;

namespace NetPack.Tests
{
    public class TypeScriptCompilePipeTests
    {

        

        [Fact]
        public async void Processes_TypescriptFiles_And_Outputs_Js_Files()
        {

            // arrange
            var mockNodeInstance = new Moq.Mock<INodeServices>();
            mockNodeInstance.Setup(a => a.InvokeAsync<TypeScriptCompileResult>(It.IsAny<string>(), It.IsAny<TypescriptCompileRequestDto>()))
                            .ReturnsAsync(new TypeScriptCompileResult() { Code = "Compiled code" });

            var mockJsRequirement = new Moq.Mock<NodeJsRequirement>();
            mockJsRequirement.Setup(a => a.Check());

            var testInputFiles = new SourceFile[]
            {
                new SourceFile(new StringFileInfo(TestUtils.TsContentOne, "somefile.ts"), "/SomeFolder")
            };

            IPipe sut = new TypeScriptCompilePipe(mockNodeInstance.Object, mockJsRequirement.Object);
            
            var pipelineContext = new Moq.Mock<IPipelineContext>();
            pipelineContext.Setup(a => a.Input).Returns(testInputFiles);

            var outputFiles = new List<SourceFile>();
            pipelineContext.Setup(a => a.AddOutput(It.IsAny<SourceFile>())).Callback<SourceFile>((a) =>
            {
                outputFiles.Add(a);
            });

            // act
            await sut.ProcessAsync(pipelineContext.Object);


            // assert

            // should output the compiled typescript file.
            Assert.Equal(outputFiles.Count, 1);
            var outputFile = outputFiles[0];

            // should name the output file .js not .ts.
            Assert.Equal(outputFile.FileInfo.Name, "somefile.js");

            // should output the file with the same directory as the input.
            Assert.Equal(outputFile.Directory, "/SomeFolder");

        }


     


    }
}