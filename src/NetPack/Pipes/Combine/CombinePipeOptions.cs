namespace NetPack.Pipes
{

    public enum SourceMapMode
    {
        None = 0,
        Inline = 1, // each source map for a file being combined, will be inlined into the index map file, produced for the combined file.
        Url = 2 // each source map for a file being combined, will be referenced in the index source map file as a url. This means the browser will have to fetch them with seperate requests.
    }
    public class JsCombinePipeOptions
    {

        public JsCombinePipeOptions()
        {
            //EnableJavascriptBundle = true;
            // EnableCssBundle = false;
            SourceMapMode = SourceMapMode.Inline;
        }

        /// <summary>
        /// When enabled, if the input files include source mapping url declarations (at the end of each file)
        /// then the combined file will produce a type of source map called an index map, which allows source mapping
        /// to work against the combined file.
        /// If this is not enabled, then source mapping url's will be stripped from the combined file.
        /// </summary>
        public SourceMapMode SourceMapMode { get; set; }

        /// <summary>
        /// The file path (including the filename) for the combined javascript file.
        /// </summary>
        public string OutputFilePath { get; set; }

       
        ///// <summary>
        ///// The filename for the combined css file.
        ///// </summary>
        //public string CombinedCssFileName { get; set; }

        //public bool EnableJavascriptBundle { get; set; }

        //public bool EnableCssBundle { get; set; }



    }
}