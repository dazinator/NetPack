using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace NetPack
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
        
        public void WatchFiles(Action<SourceFile> action)
        {
            foreach (var file in Files)
            {
                var path = file.GetPath();
                var changeToken = FileProvider.Watch(path);
                changeToken.RegisterChangeCallback((obj =>
                {
                    action(file);
                }), file);
            }
        }



    }


}