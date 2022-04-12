using NetPack.Pipeline;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NetPack.Requirements
{
    public class NpmDependenciesRequirement : IRequirement
    {
        private readonly NpmDependencyList _dependencies;

        public INetPackNodeServices NodeServices { get; }

        public NpmDependenciesRequirement(NpmDependencyList dependencies, INetPackNodeServices nodeServices)
        {
            _dependencies = dependencies;
            NodeServices = nodeServices;
        }

        public void Install(IPipeLine pipeline)
        {

            if (!_dependencies.Any())
            {
                return;
            }

            JObject deps = _dependencies.ToJObject() ?? new JObject();
            JObject packageJson = new JObject();
            packageJson.Add("dependencies", deps);

            // Todo: replace this with some form of IFileProvider so can come from other sources.
            string projectDir = NodeServices.ProjectDir;
            string overrideFilePath = Path.Combine(projectDir, "package.override.json");

            if (System.IO.File.Exists(overrideFilePath))
            {
                FileInfo fileInfo = new FileInfo(overrideFilePath);
                JObject overridePackageJson = LoadOverride(fileInfo);
                ApplyOverride(packageJson, overridePackageJson);
            }
            string packageJsonPath = Path.Combine(projectDir, "package.json");
            SavePackageJson(packageJsonPath, packageJson);

            //var workingDir = 
            RunNpmInstall(projectDir);

        }

        private void ApplyOverride(JObject packageJson, JObject overridePackageJson)
        {
            JProperty deps = overridePackageJson.Property("dependencies");
            JObject overrideDepsObject = deps.Value as JObject;
            JObject existingDeps = packageJson.Property("dependencies")?.Value as JObject;

            foreach (JProperty item in overrideDepsObject.Properties())
            {
                JProperty existingProp = existingDeps.Property(item.Name);
                if (existingProp != null)
                {
                    // override the dependency.
                    existingProp.Value = item.Value;
                }
                else
                {
                    // add the dependency.
                    existingDeps.Add(item.Name, item.Value);
                }
            }
        }

        private void SavePackageJson(string path, JObject packageJson)
        {
            // serialize JSON to a string and then write string to a file
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(packageJson));
        }

        private JObject LoadOverride(FileInfo overrides)
        {
            //todo: load deps from the package.override.json file and override them in the existing JObject.
            // This provides a mechanism for manual override of dependencies that get installed as expressed in code.
            // read JSON directly from a file
            using (StreamReader file = overrides.OpenText())
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject overridePackageJson = (JObject)JToken.ReadFrom(reader);
                    return overridePackageJson;
                }
            }

        }

        private void RunNpmInstall(string workingDir)
        {
            string args = "install";

            using (Process p = ProcessUtils.CreateNpmProcess(args, workingDir))
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
            if (obj.GetType() == GetType())
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 140743331 + EqualityComparer<Type>.Default.GetHashCode(GetType());
        }
    }
}