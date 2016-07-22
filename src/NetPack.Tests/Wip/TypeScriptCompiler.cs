using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetPack.Tests
{
    public static class TypeScriptCompiler
    {
        // helper class to add parameters to the compiler
        public class Options
        {
            private static Options _defaultOptions;
            public static Options Default
            {
                get
                {
                    if (_defaultOptions == null)
                        _defaultOptions = new Options();

                    return _defaultOptions;
                }
            }

            public enum Version
            {
                ES5,
                ES3,
            }

            public bool EmitComments { get; set; }
            public bool GenerateDeclaration { get; set; }
            public bool GenerateSourceMaps { get; set; }
            public string OutPath { get; set; }
            public Version TargetVersion { get; set; }

            public Options() { }

            public Options(bool emitComments = false
                , bool generateDeclaration = false
                , bool generateSourceMaps = false
                , string outPath = null
                , Version targetVersion = Version.ES5)
            {
                EmitComments = emitComments;
                GenerateDeclaration = generateDeclaration;
                GenerateSourceMaps = generateSourceMaps;
                OutPath = outPath;
                TargetVersion = targetVersion;
            }
        }

        public static void Compile(string tsPath, Options options = null)
        {
            if (options == null)
                options = Options.Default;

            var d = new Dictionary<string, string>();

            if (options.EmitComments)
                d.Add("-c", null);

            if (options.GenerateDeclaration)
                d.Add("-d", null);

            if (options.GenerateSourceMaps)
                d.Add("--sourcemap", null);

            if (!String.IsNullOrEmpty(options.OutPath))
                d.Add("--out", options.OutPath);

            d.Add("--target", options.TargetVersion.ToString());

            // this will invoke `tsc` passing the TS path and other
            // parameters defined in Options parameter
            Process p = new Process();

            ProcessStartInfo psi = new ProcessStartInfo("tsc", tsPath + " " + String.Join(" ", d.Select(o => o.Key + " " + o.Value)));

            // run without showing console windows
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            // redirects the compiler error output, so we can read
            // and display errors if any
            psi.RedirectStandardError = true;

            p.StartInfo = psi;

            p.Start();

            // reads the error output
            var msg = p.StandardError.ReadToEnd();

            // make sure it finished executing before proceeding 
            p.WaitForExit();

            // if there were errors, throw an exception
            if (!String.IsNullOrEmpty(msg))
                throw new InvalidTypeScriptFileException(msg);
        }
    }
}