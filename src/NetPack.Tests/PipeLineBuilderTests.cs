using Dazinator.AspNet.Extensions.FileProviders;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
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

            //var mockHostingEnv = new Mock<IHostingEnvironment>();
            //mockHostingEnv.SetupAllProperties();

            var fileProvider = new InMemoryFileProvider();

            // Enable Node Services
            services.AddNodeServices((options) =>
            {
                options.HostingModel = NodeHostingModel.Socket;
            });

        }
       
    }
}