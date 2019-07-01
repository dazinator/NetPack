using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetPack.Pipeline;
using NetPack.Process;
using System;
/// <summary>
///  Taken from https://github.com/aspnet/AspNetCore/blob/d3400f7cb23d83ae93b536a5fc9f46fc2274ce68/src/Components/Blazor/Server/src/BlazorConfig.cs
///  Which is:
///  // Copyright (c) .NET Foundation. All rights reserved.
///  //  Licensed under the Apache License, Version 2.0. See License.txt at this location for information: https://github.com/aspnet/AspNetCore/blob/master/LICENSE.txt
/// </summary>
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
            var fp = BlazorClientAppFileProviderHelper.GetContentFileProvider<TClientApp>();
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
