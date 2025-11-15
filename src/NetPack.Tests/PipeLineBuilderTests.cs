using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetPack.Pipeline;
using System.Linq;
using Dazinator.Extensions.FileProviders;
using Dazinator.Extensions.FileProviders.InMemory;
using Dazinator.Extensions.FileProviders.InMemory.Directory;
using Xunit;

namespace NetPack.Tests
{

    public class PipeLineBuilderTests
    {

        [Fact]
        public void Can_Build_Pipeline()
        {
            // arrange 
            var services = new ServiceCollection();
            services.AddLogging();
            //var mockHostingEnv = new Mock<IHostingEnvironment>();
            //mockHostingEnv.SetupAllProperties();

            var fileProvider = new InMemoryFileProvider();
            fileProvider.Directory.AddFile("wwwroot", new StringFileInfo(TsContentOne, "somefile.ts"));
            fileProvider.Directory.AddFile("wwwroot", new StringFileInfo(TsContentTwo, "someOtherfile.ts"));
            fileProvider.Directory.AddFile("wwwroot", new StringFileInfo("blah", "foo.txt"));

            // mockHostingEnv.Object.ContentRootFileProvider = fileProvider;

            //var mockFileProvider =
            //    TestUtils.GetMockFileProvider(new[] { "wwwroot/somefile.ts", "wwwroot/someOtherfile.ts" },
            //        new[] { TestUtils.TsContentOne, TestUtils.TsContentTwo });
            //mockHostingEnv.Object.ContentRootFileProvider = mockFileProvider;

            // services.AddSingleton<IHostingEnvironment>(mockHostingEnv.Object);

            var serviceProvider = services.BuildServiceProvider();
            IApplicationBuilder app = new ApplicationBuilder(serviceProvider);


            // act
            var mockPipe = new Mock<IPipe>();
            mockPipe.SetupAllProperties();

            var mockAnotherPipe = new Mock<IPipe>();
            mockAnotherPipe.SetupAllProperties();

            var sut = (IPipelineConfigurationBuilder)new PipelineConfigurationBuilder(serviceProvider, new InMemoryDirectory());

            var pipeLine = sut.WithFileProvider(fileProvider)
                .AddPipe(input =>
                {
                    input.Include("wwwroot/somefile.ts")
                        .Include("wwwroot/someOtherfile.ts");
                }, mockPipe.Object)
                .AddPipe(input =>
                {
                    input.Include("**/*.txt");
                }, mockAnotherPipe.Object)
                .BuildPipeLine();
            
            // assert
            Assert.NotNull(pipeLine);
         //   Assert.False(pipeLine.HasFlushed);
            Assert.NotNull(pipeLine.Context.GeneratedOutput);
            Assert.NotNull(pipeLine.EnvironmentFileProvider);
            Assert.NotNull(pipeLine.Context.FileProvider);
            Assert.NotNull(pipeLine.GeneratedOutputFileProvider);
            Assert.NotNull(pipeLine.Context.SourcesOutput);
            Assert.NotNull(pipeLine.SourcesFileProvider);

            Assert.NotNull(pipeLine.Pipes);
            Assert.True(pipeLine.Pipes.Count == 2);

            Assert.NotNull(pipeLine.Pipes[0].Input);
            Assert.NotNull(pipeLine.Pipes[0].Pipe);
            Assert.False(pipeLine.Pipes[0].IsProcessing);
            Assert.True(pipeLine.Pipes[0].Input.GetIncludes().Count() == 2);

            Assert.NotNull(pipeLine.Pipes[1].Input);
            Assert.NotNull(pipeLine.Pipes[1].Pipe);
            Assert.False(pipeLine.Pipes[1].IsProcessing);
            Assert.True(pipeLine.Pipes[1].Input.GetIncludes().Count() == 1);

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

        public const string TsContentTwo = @"

        class Another
        {
    constructor(public another: string) { }
    doSomething()
            {
               // return ""<h1>"" + this.greeting + ""</h1>"";
            }
        };

        var another = new Another(""Hello, world!"");
        another.doSomething();";
    }
}