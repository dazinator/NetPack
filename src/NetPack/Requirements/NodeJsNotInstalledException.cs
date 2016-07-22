using System;

namespace NetPack.Requirements
{
    public class NodeJsNotInstalledException : Exception
    {
        public NodeJsNotInstalledException() : base()
        {

        }
        public NodeJsNotInstalledException(string message) : base(message)
        {

        }
    }
}