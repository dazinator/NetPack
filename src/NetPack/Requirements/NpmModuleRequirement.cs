using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NetPack.Requirements
{
    public class NpmModuleRequirement : IRequirement
    {

        private Dictionary<string, List<string>> _installedPackages = null;

        private readonly string _packageName;
        private readonly string _version = null;
        private readonly bool _installIfNotFound = false;

        public NpmModuleRequirement(string packageName, bool installIfNotFound, string version = null)
        {
            _packageName = packageName;
            _version = version;
            _installIfNotFound = installIfNotFound;
        }

        public Dictionary<string, List<string>> GetInstalledPackages()
        {
            if (_installedPackages == null)
            {
                _installedPackages = DetectPackages();
            }

            return _installedPackages;
        }

        private Dictionary<string, List<string>> DetectPackages()
        {
            // query local npm modules, because requiring global modules within nodejs
            // requires an environment variable to be set: http://stackoverflow.com/questions/15636367/nodejs-require-a-global-module-package

            using (Process p = ProcessUtils.CreateNpmProcess("list --depth 0"))
            {
                p.Start();

                // make sure it finished executing before proceeding 
                p.WaitForExit();

                // reads the error output
                var errorMessage = p.StandardError.ReadToEnd();

                // if there were errors, throw an exception
                if (!String.IsNullOrEmpty(errorMessage))
                    throw new NodeJsNotInstalledException(errorMessage);

                var output = p.StandardOutput.ReadToEnd();
                return ParsePackagesFromStdOut(output);

            }

        }

        private Dictionary<string, List<string>> ParsePackagesFromStdOut(string output)
        {

            var packages = new Dictionary<string, List<string>>();

            var asciiEncoded = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(output));

            var reader = new StringReader(asciiEncoded);
            var line = reader.ReadLine(); // first line is environment current directory.

            var nonAsciiChars = new char[] { '?' }; // chars that could not be ascii encoded are turned into ? so we remove them.

            while (reader.Peek() != -1)
            {
                var packageLine = reader.ReadLine();
                if (packageLine.StartsWith("?"))
                {
                    //  var lastIndex = packageLine.LastIndexOf('?');
                    //  var packageName = packageLine.Substring(lastIndex + 1);
                    var packageNameWithVersion = packageLine.TrimStart(nonAsciiChars).Trim();
                    if (packageNameWithVersion == "(empty)")
                    {
                        continue;
                    }

                    string packageName;
                    string packageVersion;

                    var indexOfVersionSeperator = packageNameWithVersion.IndexOf('@');
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

                    var versionsList = packages[packageName];
                    if (!versionsList.Contains(packageVersion))
                    {
                        packages[packageName].Add(packageVersion);
                    }
                }
            }

            return packages;

        }

        public virtual void Check()
        {
            var installedPackages = GetInstalledPackages();

            if (!installedPackages.ContainsKey(_packageName))
            {
                if (_installIfNotFound)
                {
                    // try and install the requird npm module.
                    InstallNpmPackage(_packageName, _version);
                    return;
                }
            }

            // package already installed with same name, but is it the required version? 
            if (_version != null)
            {
                var installedPackageVersions = installedPackages[_packageName];
                if (!installedPackageVersions.Contains(_version))
                {
                    InstallNpmPackage(_packageName, _version);
                }
            }

        }

        private void InstallNpmPackage(string packageName, string version = null)
        {
            string args = string.IsNullOrWhiteSpace(version)
                ? $"install {packageName}"
                : $"install {packageName}@{version}";

            using (Process p = ProcessUtils.CreateNpmProcess(args))
            {
                p.Start();

                // make sure it finished executing before proceeding 
                p.WaitForExit();

                // reads the error output
                var errorMessage = p.StandardError.ReadToEnd();

                // if there were errors, throw an exception
                if (!String.IsNullOrEmpty(errorMessage))
                    throw new NpmPackageCouldNotBeInstalledException(errorMessage);

                var output = p.StandardOutput.ReadToEnd();


            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() == typeof(NpmModuleRequirement))
            {
                var req = (NpmModuleRequirement)obj;
                return req._packageName == _packageName && req._version == _version;
            }
            return false;
        }

    }
}