using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.Extensions.DependencyInjection;
using NetPack.Extensions;
using Xunit;

namespace NetPack.Tests
{
    public class ApiTests
    {
        [Fact]
        public void Can_Specify_Pipeline()
        {
            var services = new ServiceCollection();
            services.AddNetPack();

            var serviceProvider = services.BuildServiceProvider();
            IApplicationBuilder app = new ApplicationBuilder(serviceProvider);

            app.UseNetPackPipeLine(builder =>
                builder.AddTypeScriptPipe(t => { })

                );
        }
    }
}