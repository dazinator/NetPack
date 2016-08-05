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
            Files = new List<SourceFile>();
        }

        public IFileProvider FileProvider { get; set; }
        public List<SourceFile> Files { get; }
        public void AddFile(IFileInfo file, string directory)
        {
            var sourceFile = new SourceFile(file, directory);
            Files.Add(sourceFile);
        }
        
        public void WatchFiles(Action<IFileInfo> action)
        {
            foreach (var file in Files)
            {
               // var path = file.GetPath();
                WatchFile(file, action);

               

                //changeToken.RegisterChangeCallback((obj =>
                //{
                //    action(file);
                //}), file);
            }
        }

        private void WatchFile(SourceFile file, Action<IFileInfo> action)
        {
            var path = file.GetPath();
            var changeToken = FileProvider.Watch(path);
            changeToken.RegisterChangeCallback((a) =>
            {
                var newFileInfo = (IFileInfo)a;
                file.Update(newFileInfo);
                action(newFileInfo);

                // must watch file again as old change token expired.
                WatchFile(file, action);

            }, file.FileInfo);

        }
    }


}