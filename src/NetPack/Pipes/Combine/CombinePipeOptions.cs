namespace NetPack.Pipes
{
    public class JsCombinePipeOptions
    {

        public CombinePipeOptions()
        {
            //EnableJavascriptBundle = true;
           // EnableCssBundle = false;
            EnableIndexSourceMap = true;
        }

        /// <summary>
        /// When enabled, if the input files include source mapping url declarations (at the end of each file)
        /// then the combined file will produce a type of source map called an index map, which allows source mapping
        /// to work against the combined file.
        /// If this is not enabled, then source mapping url's will be stripped from the combined file.
        /// </summary>
        public bool EnableIndexSourceMap { get; set; }

        /// <summary>
        /// The filename for the combined javascript file.
        /// </summary>
        public string CombinedJsFileName { get; set; }

        ///// <summary>
        ///// The filename for the combined css file.
        ///// </summary>
        //public string CombinedCssFileName { get; set; }

        //public bool EnableJavascriptBundle { get; set; }

        //public bool EnableCssBundle { get; set; }



    }
}