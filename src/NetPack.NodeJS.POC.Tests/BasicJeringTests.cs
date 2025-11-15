using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        public async Task Test1_ForceIPv4()
        {
            // Test with IPv4 forced via DNS resolution order
            _output.WriteLine("Starting Test1_ForceIPv4");
            _output.WriteLine($"Current Directory: {Environment.CurrentDirectory}");
            
            string script = @"
                module.exports = (callback, message) => {
                    callback(null, 'Hello ' + message);
                };
            ";

            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddProvider(new XunitLoggerProvider(_output));
            });
            
            services.AddNodeJS();
            services.Configure<NodeJSProcessOptions>(options =>
            {
                options.ExecutablePath = "/usr/local/bin/node";
                options.ProjectPath = Environment.CurrentDirectory;
                
                // Force Node.js to prefer IPv4
                if (!options.EnvironmentVariables.ContainsKey("NODE_OPTIONS"))
                {
                    options.EnvironmentVariables.Add("NODE_OPTIONS", "--dns-result-order=ipv4first");
                }
                
                _output.WriteLine($"NodeJS Config - Executable: {options.ExecutablePath}");
                _output.WriteLine($"NodeJS Config - ProjectPath: {options.ProjectPath}");
                if (options.EnvironmentVariables.TryGetValue("NODE_OPTIONS", out var nodeOpts))
                {
                    _output.WriteLine($"NodeJS Config - NODE_OPTIONS: {nodeOpts}");
                }
            });
            
            var serviceProvider = services.BuildServiceProvider();
            var nodeJSService = serviceProvider.GetRequiredService<INodeJSService>();

            try
            {
                _output.WriteLine("Invoking Node.js...");
                var result = await nodeJSService.InvokeFromStringAsync<string>(script, args: new object[] { "World" });
                _output.WriteLine($"Result: {result}");
                Assert.Equal("Hello World", result);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _output.WriteLine($"Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }

    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;
        
        public XunitLoggerProvider(ITestOutputHelper output)
        {
            _output = output;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(_output, categoryName);
        }

        public void Dispose() { }
    }

    public class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper _output;
        private readonly string _categoryName;

        public XunitLogger(ITestOutputHelper output, string categoryName)
        {
            _output = output;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _output.WriteLine($"[{logLevel}] {_categoryName}: {formatter(state, exception)}");
            if (exception != null)
            {
                _output.WriteLine($"Exception: {exception}");
            }
        }
    }
}
