namespace NetPack.JsCombine
{

    public enum SourceMapMode
    {
        None = 0,
        Inline = 1, // each source map for a file being combined, will be inlined into the index map file, produced for the combined file.
        Url = 2 // each source map for a file being combined, will be referenced in the index source map file as a url. This means the browser will have to fetch them with seperate requests.
    }
}