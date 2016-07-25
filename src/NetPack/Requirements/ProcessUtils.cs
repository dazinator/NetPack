using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NetPack.Requirements
{
    public static class ProcessUtils
    {
        public static Process CreateNpmProcess(string args)
        {
            return CreateProcess("npm.cmd", args);
        }

        public static Process CreateProcess(string exeName, string args)
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
                .FirstOrDefault(x => File.Exists(x));

            return exePath;

        }

    }
}