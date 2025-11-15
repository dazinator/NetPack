using System;
using System.Threading.Tasks;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace NetPack.NodeJS.POC.Tests
{
    public class BasicJeringTests
    {
        private readonly ITestOutputHelper _output;

        public BasicJeringTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Test1_ConfiguredNodePath()
        {
            // Test with explicitly configured Node.js path
            _output.WriteLine("Starting Test1_ConfiguredNodePath");
            _output.WriteLine($"Current Directory: {Environment.CurrentDirectory}");
            
            string script = @"
                module.exports = (callback, message) => {
                    callback(null, 'Hello ' + message);
                };
            ";

            var services = new ServiceCollection();
            services.AddNodeJS();
            services.Configure<NodeJSProcessOptions>(options =>
            {
                options.ExecutablePath = "/usr/local/bin/node";  // Explicit path
                options.ProjectPath = Environment.CurrentDirectory;
                _output.WriteLine($"Configured ExecutablePath: {options.ExecutablePath}");
                _output.WriteLine($"Configured ProjectPath: {options.ProjectPath}");
            });
            
            var serviceProvider = services.BuildServiceProvider();
            var nodeJSService = serviceProvider.GetRequiredService<INodeJSService>();

            try
            {
                var result = await nodeJSService.InvokeFromStringAsync<string>(script, args: new object[] { "World" });
                _output.WriteLine($"Result: {result}");
                Assert.Equal("Hello World", result);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Exception: {ex.GetType().Name}");
                _output.WriteLine($"Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _output.WriteLine($"Inner: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        [Fact]
        public async Task Test2_SimpleCalculation()
        {
            // Test with simple math
            _output.WriteLine("Starting Test2_SimpleCalculation");
            
            string script = @"
                module.exports = (callback, x, y) => {
                    const result = x + y;
                    callback(null, result);
                };
            ";

            var services = new ServiceCollection();
            services.AddNodeJS();
            services.Configure<NodeJSProcessOptions>(options =>
            {
                options.ExecutablePath = "/usr/local/bin/node";
            });
            
            var serviceProvider = services.BuildServiceProvider();
            var nodeJSService = serviceProvider.GetRequiredService<INodeJSService>();

            var result = await nodeJSService.InvokeFromStringAsync<int>(script, args: new object[] { 3, 5 });
            _output.WriteLine($"3 + 5 = {result}");
            Assert.Equal(8, result);
        }
    }
}
