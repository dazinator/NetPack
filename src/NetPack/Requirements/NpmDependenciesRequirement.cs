using Microsoft.Extensions.FileProviders;
using NetPack.Pipeline;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetPack.Requirements
{
    public class NpmDependenciesRequirement : IRequirement
    {
        private readonly NpmDependencyList _dependencies;

        public NpmDependenciesRequirement(NpmDependencyList dependencies)
        {
            _dependencies = dependencies;
        }

        public void Install(IPipeLine pipeline)
        {

            if (!_dependencies.Any())
            {
                return;
            }

            JObject deps = _dependencies.ToJObject();
            JObject packageJson = new JObject();
            packageJson.Add("dependencies", deps);

            IFileProvider fileProvider = pipeline.EnvironmentFileProvider;
            // check for override package.json file
            IFileInfo overrides = fileProvider.GetFileInfo("/package.override.json");
            if (overrides.Exists && !overrides.IsDirectory)
            {
                ApplyOverride(packageJson, overrides);
            }

            string packageJsonPath = "package.json";
            SavePackageJson(packageJsonPath, packageJson);

            RunNpmInstall();

        }

        private void SavePackageJson(string path, JObject packageJson)
        {
            // serialize JSON to a string and then write string to a file
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(packageJson));
        }

        private void ApplyOverride(JObject deps, IFileInfo overrides)
        {

        }

        private void RunNpmInstall()
        {
            string args = "install";

            using (Process p = ProcessUtils.CreateNpmProcess(args))
            {
                p.Start();

                // make sure it finished executing before proceeding 
                p.WaitForExit();

                // reads the error output
                List<string> errors = new List<string>();
                List<string> warnings = new List<string>();

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

        public void Check(IPipeLine pipeline)
        {
            Install(pipeline);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() == this.GetType())
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 140743331 + EqualityComparer<Type>.Default.GetHashCode(this.GetType());
        }
    }
}