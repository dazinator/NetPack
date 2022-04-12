using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetPack.Pipeline;
using NetPack.Process;
using System;

namespace NetPack
{
    public static class IServiceCollectionExtensions
    {

        public static IPipelineBuilder AddProcessPipe(this IPipelineBuilder builder, Action<PipelineInputBuilder> input, Action<ProcessPipeOptions> configureOptions, string name = "Process")
        {
            var options = new ProcessPipeOptions();
            configureOptions?.Invoke(options);

            IServiceProvider appServices = builder.ServiceProvider;
            ILogger<ProcessPipe> logger = (ILogger<ProcessPipe>)appServices.GetRequiredService(typeof(ILogger<ProcessPipe>));

            var pipe = new ProcessPipe(options, logger, name);

            builder.AddPipe((inputBuilder) =>
            {
                // automatically exclude the process output report files from the pipe.
                inputBuilder.Exclude($"/**/process-results/{name}.json");
                input(inputBuilder);
            }, pipe);

            //builder.AddPipe(input, pipe);
            return builder;
        }




    }

}
