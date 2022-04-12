using System;
using NetPack.Pipeline;
using NetPack.JsCombine;

// ReSharper disable once CheckNamespace
// Extension method put in root namespace for discoverability purposes.
namespace NetPack
{

    public static class PipelineBuilderCombineExtensions
    {

        public static IPipelineBuilder AddJsCombinePipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Func<string> outputFilePath, Action<JsCombinePipeOptions> configureOptions = null)
        {
            var options = new JsCombinePipeOptions();
            var outputfile = outputFilePath();
            if (string.IsNullOrEmpty(outputfile))
            {
                throw new ArgumentNullException(nameof(outputfile));
            }
            options.OutputFilePath = outputfile;
            if (configureOptions != null)
            {
                configureOptions(options);
            }
            var pipe = new JsCombinePipe(options);

            builder.AddPipe((inputBuilder) =>
            {
                // automatically exclude the output / combined file from pipe input.
                inputBuilder.Exclude(outputfile);
                input(inputBuilder);
            }, pipe);

            builder.AddPipe(input, pipe);
            return builder;
        }

    }


}