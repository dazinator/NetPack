using System;
using System.Collections.Generic;
using NetPack.File;

namespace NetPack.Pipeline
{
    public class PipelineOutput
    {
        public PipelineOutput(List<SourceFile> files)
        {
            Files = files;
        }

        public List<SourceFile> Files { get; }

        /// <summary>
        /// Any file path that starts with the specified directory, will have its path modified, to remove that directory from its path.
        /// For example, if a file path is "somefolder/a/somefile.js" and the directory argument is "somefolder" then the files path will be chaned
        /// to "a/somefile.js".
        /// This is necessary because we can work with files based on their "content" path (ContentRootFileProvider), but when
        /// those files are resolved, we want them to be resolved relative to some other folder within the content directory, like the 
        /// wwwroot folder for example. Therefore this fixes up the paths
        /// </summary>
        /// <param name="directory"></param>
        public void NormalisePaths(string directory)
        {
            // webroot
            var parentDir = SubPathInfo.Parse(directory);

            foreach (var file in Files)
            {
                // change the directory of files, so that we remove the webroot folder they are being served, from.
                // this is because the files are sourced from the content file provider,
                // but are resolved relative to the webroot file provider.
                if (file.ContentPathInfo.Directory.ToString().StartsWith(parentDir.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var normalisedDir = file.ContentPathInfo.ToString().Substring(directory.Length);
                    normalisedDir = normalisedDir.TrimStart('/');
                    file.WebRootPathInfo = SubPathInfo.Parse(normalisedDir);
                }
            }
        }


    }
}