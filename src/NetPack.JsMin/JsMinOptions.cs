namespace NetPack.JsMin
{

    public class JsMinOptions
    {

        public JsMinOptions()
        {
            EnableSourceMaps = true;
            InlineSourceMap = false;
            InlineSources = false;
            OutputFilePath = null;
        }

        /// <summary>
        /// Whether to output source map for the minified file.
        /// </summary>
        public bool EnableSourceMaps { get; set; }

        /// <summary>
        /// Whether to inline the source map into the minified file.
        /// </summary>
        public bool InlineSourceMap { get; set; }

        /// <summary>
        /// Whether to inline the original source code into the map file.
        /// </summary>
        public bool InlineSources { get; set; }

        /// <summary>
        /// The full path to be given to the output / mminified file. Leave null to use a naming convention, which will be named after the input file, but end "min.js".
        /// </summary>
        public string OutputFilePath { get; set; }




    }
}