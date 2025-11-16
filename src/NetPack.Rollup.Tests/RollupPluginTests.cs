using System.Text.Json.Nodes;
using Xunit;

namespace NetPack.Rollup.Tests
{
    public class RollupPluginTests
    {
        [Fact]
        public void PluginConfigurationJson_Should_Decode_Unicode_Escaped_Quotes_In_Functions()
        {
            // Arrange
            var pluginOptions = new JsonObject
            {
                ["rewire"] = "FUNCfunction (moduleId, parentPath) { return lookup({ partial: moduleId, filename: parentPath, config: {baseUrl: '/amd'} }); }FUNC"
            };

            var plugin = new RollupPlugin("rollup-plugin-amd", pluginOptions, "amd", false, false);

            // Act
            var json = plugin.PluginConfigurationJson;

            // Assert
            // Should not contain Unicode escape sequences for single quotes
            Assert.DoesNotContain("\\u0027", json);
            // Should contain actual single quotes
            Assert.Contains("'/amd'", json);
            // Should contain the function without FUNC markers
            Assert.Contains("function (moduleId, parentPath)", json);
            Assert.DoesNotContain("FUNC", json);
        }

        [Fact]
        public void PluginConfigurationJson_Should_Handle_Functions_With_Single_Quotes()
        {
            // Arrange - simulate what happens when JsonSerializer escapes single quotes
            var pluginOptions = new JsonObject
            {
                ["rewire"] = "FUNCfunction (moduleId, parentPath) { return lookup({ partial: moduleId, filename: parentPath, config: {baseUrl: '/hmr/amd'} }); }FUNC"
            };

            var plugin = new RollupPlugin("rollup-plugin-amd", pluginOptions, "amd", false, false);

            // Act
            var json = plugin.PluginConfigurationJson;

            // Assert
            // Should not contain Unicode escape sequences
            Assert.DoesNotContain("\\u0027", json);
            // Should contain readable JavaScript with single quotes
            Assert.Contains("'/hmr/amd'", json);
        }
    }
}
