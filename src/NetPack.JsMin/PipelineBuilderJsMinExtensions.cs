using System;
using NetPack.Pipeline;
using NetPack.JsMin;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{

    public static class PipelineBuilderJsMinExtensions
    {

        public static IPipelineBuilder AddJsMinPipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Action<JsMinOptions> configureOptions = null)
        {
            var options = new JsMinOptions();
            if (configureOptions != null)
            {
                configureOptions(options);
            }
            var pipe = new JsMinifierPipe(options);

            builder.AddPipe((inputBuilder) =>
            {
                // automatically exclude the output / combined file from pipe input.
                inputBuilder.Exclude("/**/*.min.js");
                input(inputBuilder);
            }, pipe);

            // builder.AddPipe(input, pipe);
            return builder;
        }

    }
}