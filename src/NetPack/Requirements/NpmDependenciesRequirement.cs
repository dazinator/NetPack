using NetPack.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

namespace NetPack.Requirements
{
    public class NpmDependenciesRequirement : IRequirement
    {
        private readonly NpmDependencyList _dependencies;

        private INetPackNodeServices NodeServices { get; }

        public NpmDependenciesRequirement(NpmDependencyList dependencies, INetPackNodeServices nodeServices)
        {
            _dependencies = dependencies;
            NodeServices = nodeServices;
        }

        private void Install(IPipeLine pipeline)
        {
            if (!_dependencies.Any())
            {
                return;
            }

            var deps = _dependencies.ToJsonObject() ?? new JsonObject();
            var packageJson = new JsonObject();
            packageJson.Add("dependencies", deps);

            // Todo: replace this with some form of IFileProvider so can come from other sources.
            var projectDir = NodeServices.ProjectDir;
            var overrideFilePath = Path.Combine(projectDir, "package.override.json");

            if (System.IO.File.Exists(overrideFilePath))
            {
                var fileInfo = new FileInfo(overrideFilePath);
                var overridePackageJson = LoadOverride(fileInfo);
                ApplyOverride(packageJson, overridePackageJson);
            }
            var packageJsonPath = Path.Combine(projectDir, "package.json");
            SavePackageJson(packageJsonPath, packageJson);

            //var workingDir = 
            RunNpmInstall(projectDir);
        }

        private void ApplyOverride(JsonObject packageJson, JsonObject overridePackageJson)
        {
            if (overridePackageJson["dependencies"] is not JsonObject overrideDepsObject ||
                packageJson["dependencies"] is not JsonObject existingDeps)
            {
                return;
            }

            foreach (var item in overrideDepsObject)
            {
                if (existingDeps.TryGetPropertyValue(item.Key, out var existingProp))
                {
                    // override the dependency.
                    existingDeps[item.Key] = item.Value;
                }
                else
                {
                    // add the dependency.
                    existingDeps.Add(item.Key, item.Value);
                }
            }
        }
        
        private void SavePackageJson(string path, JsonObject packageJson)
        {
            // serialize JSON to a string and then write string to a file
            var jsonString = packageJson.ToJsonString();
            System.IO.File.WriteAllText(path, jsonString);
        }
     

        private JsonObject LoadOverride(FileInfo overrides)
        {
            //todo: load deps from the package.override.json file and override them in the existing JsonObject.
            // This provides a mechanism for manual override of dependencies that get installed as expressed in code.
            // read JSON directly from a file
            using var file = overrides.OpenText();
            var jsonString = file.ReadToEnd();
            var overridePackageJson = JsonNode.Parse(jsonString)?.AsObject();
            return overridePackageJson;
        }

        private void RunNpmInstall(string workingDir)
        {
            const string args = "install";

            using var p = ProcessUtils.CreateNpmProcess(args, workingDir);
            p.Start();

            // make sure it finished executing before proceeding 
            p.WaitForExit();

            // reads the error output
            var errors = new List<string>();
            var warnings = new List<string>();

            while (!p.StandardError.EndOfStream)
            {
                var line = p.StandardError.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

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

            if (errors.Count != 0)
            {
                var errorMessage = string.Join(Environment.NewLine, errors);
                // if there were errors, throw an exception
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    throw new NpmPackageCouldNotBeInstalledException(errorMessage);
                }
            }
            var output = p.StandardOutput.ReadToEnd();
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

            return obj.GetType() == GetType();
        }

        public override int GetHashCode()
        {
            return 140743331 + EqualityComparer<Type>.Default.GetHashCode(GetType());
        }
    }
}