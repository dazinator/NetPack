using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using NetPack.File;

namespace NetPack.Pipeline
{
    public class PipelineInput
    {
        public PipelineInput(IFileProvider fileProvider)
        {
            FileProvider = fileProvider;
            Files = new List<string>();
        }

        public IFileProvider FileProvider { get; set; }
        public List<string> IncludeList { get; }
        public void AddInclude(string pattern)
        {
            IncludeList.Add(pattern);
        }

        public void WatchFiles(Action<string> action)
        {
            foreach (var file in IncludeList)
            {
                // var path = file.GetPath();
                WatchFile(file, action);



                //changeToken.RegisterChangeCallback((obj =>
                //{
                //    action(file);
                //}), file);
            }
        }

        private void WatchFile(string pattern, Action<string> action)
        {
            // var path = file.ContentPathInfo.ToString();
            var changeToken = FileProvider.Watch(pattern);
            changeToken.RegisterChangeCallback((a) =>
            {
                // var newFileInfo = (IFileInfo)a;
                //  file.Update(newFileInfo);
                action(pattern);
                // must watch file again as old change token expired.
                WatchFile(pattern, action);

            }, pattern);

        }
    }


}