using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;

namespace NetPack.Process
{
    public class ProcessPipeOptions
    {
        public ProcessPipeOptions()
        {
            Arguments = new List<string>();
            EnvironmentVariables = new Dictionary<string, string>();
        }

        public string ExecutableName { get; set; }

        public string WorkingDirectory { get; set; }

        public string StdIn { get; set; }
        public List<string> Arguments { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; }
        public ProcessPipeOptions AddArgument(string arg)
        {
            Arguments.Add(arg);
            return this;
        }
        public ProcessPipeOptions SetEnvironmentVariable(string name, string value)
        {
            if (!EnvironmentVariables.ContainsKey(name))
            {
                EnvironmentVariables.Add(name, value);
                return this;
            }

            EnvironmentVariables[name] = value;
            return this;
        }

        public Action<string> StdOutCallback { get; set; }
        public Action<string> StdErrCallback { get; set; }


        public bool ThrowExceptionOnNonZeorExitCode { get; set; } = true;
        public bool ThrowExceptionOnNonEmptyStdErr { get; set; } = false;



    }

}
