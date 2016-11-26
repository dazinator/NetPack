using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Pipes;
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

            var mockHostingEnv = new Mock<IHostingEnvironment>();
            mockHostingEnv.SetupAllProperties();

            var fileProvider = new InMemoryFileProvider();
            fileProvider.Directory.AddFile("wwwroot", new StringFileInfo(TsContentOne, "somefile.ts"));
            fileProvider.Directory.AddFile("wwwroot", new StringFileInfo(TsContentTwo, "someOtherfile.ts"));

            mockHostingEnv.Object.ContentRootFileProvider = fileProvider;

            //var mockFileProvider =
            //    TestUtils.GetMockFileProvider(new[] { "wwwroot/somefile.ts", "wwwroot/someOtherfile.ts" },
            //        new[] { TestUtils.TsContentOne, TestUtils.TsContentTwo });
            //mockHostingEnv.Object.ContentRootFileProvider = mockFileProvider;

            services.AddSingleton<IHostingEnvironment>(mockHostingEnv.Object);

            var serviceProvider = services.BuildServiceProvider();
            IApplicationBuilder app = new ApplicationBuilder(serviceProvider);


            // act
            var mockPipe = new Mock<IPipe>();
            mockPipe.SetupAllProperties();

            var mockAnotherPipe = new Mock<IPipe>();
            mockAnotherPipe.SetupAllProperties();

            var sut = (IPipelineConfigurationBuilder)new PipelineConfigurationBuilder(app);

            var pipeLine = sut.Take((inputBuilder) =>
                                             inputBuilder.Include("wwwroot/somefile.ts")
                                                         .Include("wwwroot/someOtherfile.ts"))
                                                         .Watch()
                                                         .BeginPipeline()
                                                            .AddPipe(mockPipe.Object)
                                                            .AddPipe(mockAnotherPipe.Object)
                                                         .BuildPipeLine();

            // assert
            Assert.NotNull(pipeLine);
            Assert.NotNull(pipeLine.Input);
            Assert.True(pipeLine.Input.Files.Count == 2);

            Assert.False(pipeLine.HasFlushed);
           // Assert.True(pipeLine.IsWatching);

            Assert.NotNull(pipeLine.Pipes);
            Assert.True(pipeLine.Pipes.Count == 2);

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