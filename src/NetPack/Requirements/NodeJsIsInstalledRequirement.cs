using NetPack.Pipeline;
using System;
using System.Diagnostics;

namespace NetPack.Requirements
{
    public class NodeJsIsInstalledRequirement : IRequirement
    {
        public virtual void Check(IPipeLine pipeline)
        {

            using (Process p = new Process())
            {
                ProcessStartInfo psi = new ProcessStartInfo("node", "-v");

                // run without showing console windows
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;

                // redirects the compiler error output, so we can read
                // and display errors if any
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;

                p.StartInfo = psi;

                try
                {

                }
                catch (Exception e)
                {

                    throw;
                }
                p.Start();

                // reads the error output
                var errorMessage = p.StandardError.ReadToEnd();
                var output = p.StandardOutput.ReadToEnd();

                // make sure it finished executing before proceeding 
                p.WaitForExit();

                // if there were errors, throw an exception
                if (!String.IsNullOrEmpty(errorMessage))
                    throw new NodeJsNotInstalledException(errorMessage);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() == typeof(NodeJsIsInstalledRequirement))
            {
                return true;
            }
            return false;
        }
    }
}