namespace NetPack.Pipeline
{
    //public class PipelineOutput
    //{
    //    private string requestPath;

    //    public PipelineOutput(List<SourceFile> files)
    //    {
    //        Files = files;
            
    //    }

    //    public PipelineOutput(List<SourceFile> files, string requestPath) : this(files)
    //    {
    //        this.requestPath = requestPath;
    //        SetRequestPaths(requestPath);
    //    }

    //    public List<SourceFile> Files { get; }

    //    /// <summary>
    //    /// Will set all of the output files in the pipeline to have a "RequestPath" that is equal to their current content path, but prefixed with the specified
    //    /// base path. So if a file's content path is '/content/myfile.js' then calling this method with a baseRequestPath arg of '/netpack' will
    //    /// result in the file having a request path of '/netpack/content/myfile.js'.
    //    /// 
    //    /// The request path is the path that you can resolve the file on via a http request.
    //    ///  </summary>
    //    /// <param name="baseRequestPath"></param>
    //    public void SetRequestPaths(string baseRequestPath)
    //    {
    //        // webroot
    //        var basePath = SubPathInfo.Parse(baseRequestPath);

    //        foreach (var file in Files)
    //        {
    //           // file.WebPathInfo = SubPathInfo.Parse(basePath.ToString() + "/" + file.ContentPathInfo.ToString());

    //            if (file.WebPathInfo == null)
    //            {
    //                file.WebPathInfo = SubPathInfo.Parse(file.ContentPathInfo.ToString());
    //            }

    //            // change the directory of files, so that we remove the webroot folder they are being served, from.
    //            // this is because the files are sourced from the content file provider,
    //            // but are resolved relative to the webroot file provider.
    //            //if (file.ContentPathInfo.Directory.ToString().StartsWith(parentDir.ToString(), StringComparison.OrdinalIgnoreCase))
    //            //{
    //            //    var normalisedDir = file.ContentPathInfo.ToString().Substring(basePath.Length);
    //            //    normalisedDir = normalisedDir.TrimStart('/');
    //            //    file.WebPathInfo = SubPathInfo.Parse(normalisedDir);
    //            //}
    //        }
    //    }


    //}
}