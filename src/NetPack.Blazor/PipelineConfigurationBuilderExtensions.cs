using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetPack.Pipeline;
using NetPack.Process;
using System;

namespace NetPack.Blazor
{
    public static class PipelineConfigurationBuilderExtensions
    {
        public static IPipelineBuilder WithBlazorClientWebRootFileProvider<TClientApp>(this IPipelineConfigurationBuilder pipelineBuilder)
        {
            var fp = BlazorClientAppFileProviderHelper.GetStaticFileProvider<TClientApp>();
            return pipelineBuilder.WithFileProvider(fp);
        }
        public static IPipelineBuilder WithBlazorClientContentFileProvider<TClientApp>(this IPipelineConfigurationBuilder pipelineBuilder)
        {
            var fp = BlazorClientAppFileProviderHelper.GetProjectFileProvider<TClientApp>();
            return pipelineBuilder.WithFileProvider(fp);
        }

        public static IPipelineBuilder AddBlazorRecompilePipe<TStartup>(this IPipelineBuilder builder, Action<ProcessPipeOptions> configureOptions = null, string name = "Process")
        {

            IServiceProvider appServices = builder.ServiceProvider;
            ILogger<ProcessPipe> logger = (ILogger<ProcessPipe>)appServices.GetRequiredService(typeof(ILogger<ProcessPipe>));

            builder.AddProcessPipe((input) =>
             {
                 input.Include("/**/*.razor");
             }, (processOptions) =>
             {                 
                 processOptions.ExecutableName = "dotnet.exe";
                 var projDir = BlazorClientAppFileProviderHelper.GetBlazorClientProjectDirectory<TStartup>(out string outputAssemblyPath);
                 processOptions.WorkingDirectory = projDir;
                 processOptions.AddArgument("build");
                // processOptions.AddArgument("-p:BlazorLinkOnBuild=false"); // disable linker whilst debugging
                 Action<string> logOutput = new Action<string>((a) =>
                 {
                     logger.LogDebug(a.ToString());
                 });
                 Action<string> logErrorCallback = new Action<string>((a) =>
                 {
                     logger.LogError(a.ToString());
                 });
                 processOptions.StdOutCallback = logOutput;
                 processOptions.StdErrCallback = logErrorCallback;
                 configureOptions?.Invoke(processOptions);
             });

            return builder;
        }


    }
}
