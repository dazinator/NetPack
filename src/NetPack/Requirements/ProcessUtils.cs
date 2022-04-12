using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NetPack.Requirements
{
    public static class ProcessUtils
    {
        public static Process CreateNpmProcess(string args, string workingDirectory = null)
        {
            return CreateProcess("npm.cmd", args, workingDirectory);
        }

        public static Process CreateProcess(string exeName, string args, string workingDirectory = null)
        {
            var process = new Process();
            var npmExePath = ResolveExecutablePathFromEnvironmentPath(exeName);
            if (string.IsNullOrWhiteSpace(npmExePath))
            {
                throw new Exception($"{exeName} is not available from the current PATH.");
            }
            ProcessStartInfo psi = new ProcessStartInfo(npmExePath, args);

            // run without showing console windows
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
            // redirects the compiler error output, so we can read
            // and display errors if any
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            process.StartInfo = psi;
            return process;
        }

        private static string ResolveExecutablePathFromEnvironmentPath(string exe)
        {
            var enviromentPath = System.Environment.GetEnvironmentVariable("PATH");
            var paths = enviromentPath.Split(';');

            var exePath = paths
                .Select(x => Path.Combine(x, exe))
                .FirstOrDefault(x => System.IO.File.Exists(x));

            return exePath;

        }

    }
}