using NetPack.Requirements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetPack.Utils
{
    public static class NpmUtils
    {
        public static Dictionary<string, List<string>> ParsePackagesFromStdOut(string output)
        {

            var packages = new Dictionary<string, List<string>>();

            var asciiEncoded = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(output));

            var reader = new StringReader(asciiEncoded);
            var line = reader.ReadLine(); // first line is environment current directory.

            var nonAsciiChars = new char[] { '?' }; // chars that could not be ascii encoded are turned into ? so we remove them.

            while (reader.Peek() != -1)
            {
                var packageLine = reader.ReadLine();
                if (packageLine.StartsWith("?") || packageLine.StartsWith("+--"))
                {
                    //  var lastIndex = packageLine.LastIndexOf('?');
                    //  var packageName = packageLine.Substring(lastIndex + 1);
                    string packageNameWithVersion = packageLine.TrimStart(nonAsciiChars).Trim();
                    packageNameWithVersion = packageNameWithVersion.TrimStart(new char[] { '+', '-', '-' }).Trim();

                    if (packageNameWithVersion == "(empty)")
                    {
                        continue;
                    }

                    string packageName;
                    string packageVersion;

                    int indexOfVersionSeperator = packageNameWithVersion.IndexOf('@');
                    if (indexOfVersionSeperator != -1)
                    {
                        packageVersion = packageNameWithVersion.Substring(indexOfVersionSeperator + 1);
                        packageName = packageNameWithVersion.Substring(0, indexOfVersionSeperator);
                    }
                    else
                    {
                        packageVersion = "";
                        packageName = packageNameWithVersion;
                    }

                    if (!packages.ContainsKey(packageName))
                    {
                        packages.Add(packageName, new List<string>());
                    }

                    List<string> versionsList = packages[packageName];
                    if (!versionsList.Contains(packageVersion))
                    {
                        packages[packageName].Add(packageVersion);
                    }
                }
            }

            return packages;

        }

        public static Dictionary<string, List<string>> DetectPackages()
        {
            // query local npm modules, because requiring global modules within nodejs
            // requires an environment variable to be set: http://stackoverflow.com/questions/15636367/nodejs-require-a-global-module-package
            using (var p = ProcessUtils.CreateNpmProcess("list --depth 0"))
            {


                var errors = new List<string>();
                var warnings = new List<string>();

                var output = new StringBuilder();
                // StringBuilder error = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                {
                    p.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };

                    p.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            // outputWaitHandle.Set();
                        }
                        else
                        {
                            string line = e.Data;
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                if (line.Contains("WARN"))
                                {
                                    warnings.Add(line);
                                    // WARNINGS ARE OK.
                                }
                                else if (line.StartsWith("npm ERR! extraneous"))
                                {
                                    warnings.Add(line);
                                }
                                else
                                {
                                    errors.Add(line);
                                }
                            }

                        }
                    };

                    p.Start();

                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    p.WaitForExit();

                    outputWaitHandle.WaitOne();
                }

                if (!errors.Any())
                {
                    string outputText = output.ToString();
                    return ParsePackagesFromStdOut(outputText);
                }
                else
                {                   
                    string errorMessage = string.Join(Environment.NewLine, errors);
                    throw new NodeJsNotInstalledException(errorMessage);
                }
            }
        }

        public static void InstallNpmPackage(string packageName, string version = null)
        {
            string args = string.IsNullOrWhiteSpace(version)
                ? $"install {packageName}"
                : $"install {packageName}@{version}";

            using (var p = ProcessUtils.CreateNpmProcess(args))
            {
                p.Start();

                // make sure it finished executing before proceeding 
                p.WaitForExit();

                // reads the error output
                var errors = new List<string>();
                var warnings = new List<string>();

                while (!p.StandardError.EndOfStream)
                {
                    string line = p.StandardError.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (line.Contains("WARN"))
                        {
                            warnings.Add(line);
                            // WARNINGS ARE OK.
                        }
                        else if (line.StartsWith("npm notice"))
                        {
                            warnings.Add(line);
                        }
                        else
                        {
                            errors.Add(line);
                        }
                    }
                }

                if (errors.Any())
                {
                    string errorMessage = string.Join(Environment.NewLine, errors);
                    // if there were errors, throw an exception
                    if (!String.IsNullOrEmpty(errorMessage))
                    {
                        throw new NpmPackageCouldNotBeInstalledException(errorMessage);
                    }
                }

                string output = p.StandardOutput.ReadToEnd();

            }
        }
    }
}