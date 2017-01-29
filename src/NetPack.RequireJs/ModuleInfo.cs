using System.Collections.Generic;

namespace NetPack.RequireJs
{

    public class ModuleInfo
    {
        public ModuleInfo()
        {
            Exclude = new List<string>();
            ExcludeShallow = new List<string>();
            Include = new List<string>();
        }
        /// <summary>
        /// The name of the AMD modul - i.e "foo/bar/bop",
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// create: true can be used to create the module layer at the given
        /// name, if it does not already exist in the source location. If
        /// there is a module at the source location with this name, then
        /// create: true is superfluous.
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// The modules that should be excluded from the built file (including that modules dependencies)
        /// </summary>
        public List<string> Exclude { get; set; }

        /// <summary>
        /// Used to specify a specific module be excluded
        /// from the built module file. excludeShallow means just exclude that
        /// specific module, but if that module has nested dependencies that are
        /// part of the built file, keep them in there. This is useful during
        /// development when you want to have a fast bundled set of modules, but
        /// just develop/debug one or two modules at a time.
        /// </summary>
        public List<string> ExcludeShallow { get; set; }


        /// <summary>
        /// This moduels whose dependencies (and their depedencies etc) should be combined into one file.
        /// </summary>
        public List<string> Include { get; set; }

        /// <summary>
        /// Shows the use insertRequire (first available in 2.0):
        /// </summary>
        public string InsertRequire { get; set; }


    }
}