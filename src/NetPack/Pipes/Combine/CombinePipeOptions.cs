namespace NetPack.Pipes
{
    public class CombinePipeOptions
    {
        /// <summary>
        /// When enabled, if the files to be bundled have source maps, then the combined file
        /// will preserve the source map information for each concatenated file.
        /// </summary>
        public bool PreserveSourceMaps { get; set; }

    }
}