using System;

namespace NetPack.Requirements
{
    public class NpmPackageCouldNotBeInstalledException : Exception
    {
        public NpmPackageCouldNotBeInstalledException() : base()
        {

        }
        public NpmPackageCouldNotBeInstalledException(string message) : base(message)
        {

        }
    }
}