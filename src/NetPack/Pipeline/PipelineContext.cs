using Microsoft.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.Directory;
using Microsoft.AspNetCore.Http;

namespace NetPack.Pipeline
{

    public class PipelineContext : IPipelineContext
    {

        public PipelineContext(IFileProvider fileProvider, IDirectory sourceOutput) : this(fileProvider, sourceOutput, new InMemoryDirectory())
        {

        }

        public PipelineContext(IFileProvider fileProvider, IDirectory sourceOutput, IDirectory directory) : this(fileProvider, sourceOutput, directory, string.Empty)
        {

        }

        public PipelineContext(IFileProvider fileProvider, IDirectory sourceOutput, IDirectory directory, string baseRequestPath)
        {
            FileProvider = fileProvider;
            SourcesOutput = sourceOutput;
            ProcessedOutput = directory;
            BaseRequestPath = baseRequestPath;
            //Input = input;
        }

        public PathString BaseRequestPath { get; }

        /// <summary>
        /// Adds the generated file as an output of the pipeline.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="file"></param>
        /// <param name="excludeFromInput">Whether to add the output ile to the exclude list of this pipelines input. True by default, prevents the pipeline from processing a file which it has generated.</param>
        public void AddOutput(string directory, IFileInfo file, bool excludeFromInput = true)
        {
            //if (excludeFromInput)
            //{
            //    //to-do - check for concurrency?
            //    Input.AddExclude($"{directory}/{file.Name}");
            //}
            ProcessedOutput.AddOrUpdateFile(directory, file);
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

        public IDirectory ProcessedOutput { get; set; }

        public IDirectory SourcesOutput { get; set; }

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

        public PipelineInput Input { get; set; }

        public bool HasChanges { get; private set; }

        public void SetInput(PipelineInput input)
        {
            Input = input;
            SetInput(FileProvider.GetFiles(input));
            // ExcludeFiles = FileProvider.GetFiles(input.e)
        }

        private void SetInput(FileWithDirectory[] input)
        {
            // grab the input files from the file provider.
            PreviousInputFiles = InputFiles;
            InputFiles = input;
            HasChanges = false;
            foreach (var item in InputFiles)
            {
                if (IsDifferentFromLastTime(item))
                {
                    HasChanges = true;
                    break;
                }
            }

        }

        /// <summary>
        /// Any file added here will be able to be served up to the browser, but will not trigger
        /// any additional processing, as it's assumed to be a source file which is not modified, and for which some generated 
        /// processed file was produced. Usually you want this method if you need to expose the original source file because source maps are
        /// being used.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileInfo"></param>
        public void AddSourceOutput(string directory, IFileInfo fileInfo)
        {
            this.SourcesOutput.AddOrUpdateFile(directory, fileInfo);
            //  throw new NotImplementedException();
        }
    }
}