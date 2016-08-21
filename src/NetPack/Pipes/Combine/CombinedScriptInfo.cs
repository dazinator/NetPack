using NetPack.File;

namespace NetPack.Pipes
{
    public class CombinedScriptInfo
    {
        /// <summary>
        /// The number of lines in this script.
        /// </summary>
        public int LineCount { get; set; }
        /// <summary>
        /// If this script has a source mapping url declaration - this is it. Otherwise null.
        /// </summary>
        public SourceMappingUrlDeclaration SourceMapDeclaration { get; set; }

        /// <summary>
        /// The number of lines, from line 1, where this script starts, in the combinded script.
        /// </summary>
        public int LineNumberOffset { get; set; }
        /// <summary>
        /// The path to the script.
        /// </summary>
        public SubPathInfo Path { get; set; }
    }
}