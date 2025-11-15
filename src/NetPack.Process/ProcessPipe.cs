using CliWrap;
using Microsoft.Extensions.Logging;
using NetPack.Pipeline;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.Extensions.FileProviders;

namespace NetPack.Process
{

    public class ProcessPipe : BasePipe
    {
        private readonly ILogger<ProcessPipe> _logger;
        private readonly ProcessPipeOptions _options;

        public ProcessPipe(ProcessPipeOptions options, ILogger<ProcessPipe> logger, string name = "Process") : base(name)
        {
            _logger = logger;
            _options = options;
        }


        public override async Task ProcessAsync(PipeState context, CancellationToken cancelationToken)
        {
            try
            {

                var opt = _options;

                var builder = Cli.Wrap(opt.ExecutableName)
                                .SetCancellationToken(cancelationToken)
                                .SetArguments(opt.Arguments);

                foreach (var item in opt.EnvironmentVariables)
                {
                    builder.SetEnvironmentVariable(item.Key, item.Value);
                }

                if (opt.StdOutCallback != null)
                {
                    builder.SetStandardOutputCallback(opt.StdOutCallback);
                }

                if (opt.StdErrCallback != null)
                {
                    builder.SetStandardErrorCallback(opt.StdErrCallback);
                }

                if (!string.IsNullOrWhiteSpace(opt.WorkingDirectory))
                {
                    builder.SetWorkingDirectory(opt.WorkingDirectory);
                }

                builder.EnableExitCodeValidation(opt.ThrowExceptionOnNonZeorExitCode);
                builder.EnableStandardErrorValidation(opt.ThrowExceptionOnNonEmptyStdErr);

                var res = await builder.ExecuteAsync();

                var exitCode = res.ExitCode;
                var stdOut = res.StandardOutput;
                var stdErr = res.StandardError;
                var startTime = res.StartTime;
                var exitTime = res.ExitTime;
                var runTime = res.RunTime;

                GenerateOutput(context, exitCode, stdOut, stdErr, startTime, exitTime, runTime);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing process pipe.");
                // throw;
            }
        }

        private void GenerateOutput(PipeState context, int exitCode, string output, string error, DateTimeOffset startTime, DateTimeOffset exitTime, TimeSpan runtime)
        {
            var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, System.Text.Encoding.UTF8, 1024, true))
            {
                streamWriter.WriteLine("{");
                streamWriter.WriteLine($"\"ExitCode\": \"{exitCode}\"");
                streamWriter.Write($"\"Output\": \"{output}\"");
                streamWriter.Write($"\"Error\": \"{error}\"");
                streamWriter.Write($"\"StartTime\": \"{startTime}\"");
                streamWriter.Write($"\"ExitTime\": \"{exitTime}\"");
                streamWriter.Write($"\"RunTime\": \"{runtime}\"");
                streamWriter.WriteLine("}");

                context.AddOutput("/process-results", new MemoryStreamFileInfo(memoryStream, Name + ".json"));
            }
        }
    }

}
