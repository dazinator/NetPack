using System;
using System.Collections.Generic;
using NetPack.File;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.AspNetCore.Http;

namespace NetPack.Pipeline
{

    public class PipelineContext : IPipelineContext
    {

        public PipelineContext(IFileProvider fileProvider) : this(fileProvider, new InMemoryDirectory())
        {
        }

        public PipelineContext(IFileProvider fileProvider, IDirectory directory) : this(fileProvider, directory, string.Empty)
        {
        }

        public PipelineContext(IFileProvider fileProvider, IDirectory directory, string baseRequestPath)
        {
            FileProvider = fileProvider;
            Output = directory;
            BaseRequestPath = baseRequestPath;
            //Input = input;
        }

        public PathString BaseRequestPath { get; }

        public void AddOutput(string directory, IFileInfo file)
        {
            Output.AddOrUpdateFile(directory, file);
            // return new FileWithDirectory(directory, file);
            //  Output.AddFile(directory, info);
        }

        public PathString GetRequestPath(string directory, IFileInfo fileInfo)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return BaseRequestPath.Add("/" + fileInfo.Name);
            }

            if (!directory.StartsWith("/"))
            {
                return BaseRequestPath.Add($"/{directory}").Add("/" + fileInfo.Name);
            }

            return BaseRequestPath.Add(directory).Add("/" + fileInfo.Name);
            //return BaseRequestPath.Add(directory).Add(fileInfo.Name);
        }

        public IDirectory Output { get; set; }

        public IFileProvider FileProvider { get; set; }

        public FileWithDirectory[] PreviousInputFiles { get; set; }

        public FileWithDirectory GetPreviousVersionOfFile(FileWithDirectory fileWithDirectory)
        {
            if (PreviousInputFiles == null)
            {
                return null;
            }

            foreach (var item in PreviousInputFiles)
            {
                if (item.FileSubPath == fileWithDirectory.FileSubPath)
                {
                    return item;
                }
            }

            return null;
        }

        public bool IsDifferentFromLastTime(FileWithDirectory fileWithDirectory)
        {
            var oldFile = GetPreviousVersionOfFile(fileWithDirectory);
            if (oldFile == null)
            {
                // We don't have a previous version of this file, so the file must be a new version.
                return true;
            }

            // Return whether this version of the file has a gretaer modified date than the last version of this file.
            var changed = fileWithDirectory.FileInfo.LastModified > oldFile.FileInfo.LastModified;
            return changed;

        }

        public FileWithDirectory[] InputFiles { get; set; }

        public void SetInput(PipelineInput input)
        {            
            SetInput(FileProvider.GetFiles(input));
        }

        public void SetInput(FileWithDirectory[] input)
        {
            // grab the input files from the file provider.
            PreviousInputFiles = InputFiles;
            InputFiles = input;
        }




    }
}