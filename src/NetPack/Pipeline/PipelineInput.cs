using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using Dazinator.AspNet.Extensions.FileProviders;
using System.Linq;

namespace NetPack.Pipeline
{
    public class PipelineInput
    {
        public PipelineInput()
        {
            //  FileProvider = fileProvider;
            IncludeList = new List<string>();
        }

        //  public IFileProvider FileProvider { get; set; }
        public List<string> IncludeList { get; }
        public void AddInclude(string pattern)
        {
            IncludeList.Add(pattern);
            LastChanged = DateTime.UtcNow;
        }

        public DateTime LastChanged { get; set; }

        public FileWithDirectory[] GetFiles(IFileProvider fileProvider)
        {
            var results = new Dictionary<string, FileWithDirectory>();
            foreach (var input in IncludeList)
            {
                //string searchPattern = input;
                //if (!input.StartsWith("/"))
                //{
                //    searchPattern = "/" + input;
                //}
                var files = fileProvider.Search(input);
                // check if file already present? Multiple input patterns can match the same files.
                foreach (var file in files)
                {
                    var path = $"{file.Item1}/{file.Item2.Name}";
                    if (!results.ContainsKey(path))
                    {
                        var item = new FileWithDirectory() { Directory = file.Item1, FileInfo = file.Item2 };
                        results.Add(path, item);
                    }
                }

            }
            return results.Values.ToArray();
        }


        //public void WatchFiles(Action<string> action)
        //{
        //    foreach (var file in IncludeList)
        //    {
        //        // var path = file.GetPath();
        //        WatchFile(file, action);



        //        //changeToken.RegisterChangeCallback((obj =>
        //        //{
        //        //    action(file);
        //        //}), file);
        //    }
        //}

        //private void WatchFile(string pattern, Action<string> action)
        //{
        //    // var path = file.ContentPathInfo.ToString();
        //    var changeToken = FileProvider.Watch(pattern);
        //    changeToken.RegisterChangeCallback((a) =>
        //    {
        //        // var newFileInfo = (IFileInfo)a;
        //        //  file.Update(newFileInfo);
        //        action(pattern);
        //        // must watch file again as old change token expired.
        //        WatchFile(pattern, action);

        //    }, pattern);

        //}
    }


}