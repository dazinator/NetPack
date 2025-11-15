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
            // On Windows, npm is npm.cmd, on Linux/Mac it's just npm
            var npmExe = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) 
                ? "npm.cmd" 
                : "npm";
            return CreateProcess(npmExe, args, workingDirectory);
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
            if(!string.IsNullOrWhiteSpace(workingDirectory))
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
            // On Windows, PATH uses ';' as separator, on Linux/Mac it uses ':'
            var pathSeparator = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) 
                ? ';' 
                : ':';
            var paths = enviromentPath.Split(pathSeparator);

            var exePath = paths
                .Select(x => Path.Combine(x, exe))
                .FirstOrDefault(x => System.IO.File.Exists(x));

            return exePath;

        }

    }
}