using System;
using NetPack.RequireJs;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace NetPack.Pipeline
{

    public class PipeContext
    {

        public PipeContext(PipelineInput input, IPipe pipe)
        {
            Input = input;
            Pipe = pipe;
        }

        public IPipe Pipe { get; set; }
        public PipelineInput Input { get; set; }
        public DateTime LastProcessedEndTime { get; set; } = DateTime.MinValue.ToUniversalTime();
        public DateTime LastProcessStartTime { get; set; } = DateTime.MinValue.ToUniversalTime();
        public bool IsProcessing { get; set; }
        /// <summary>
        /// Returns true if the pipe has updated inputs that need to be processed.
        /// </summary>
        /// <returns></returns>
        internal bool IsDirty()
        {
            var isDirty = (Input.LastChanged > LastProcessStartTime);
           // isDirty = isDirty && Input.LastChanged <= LastProcessedEndTime; // inputs were changed in between last processing start and end time.
            //isDirty = isDirty || Input.LastChanged > LastProcessedEndTime; // or inputs have been changed since last processing time ended.

            return isDirty;
        }

        /// <summary>
        /// Returns the previous files that were processed.
        /// </summary>
        FileWithDirectory[] PreviousInputFiles { get; set; }

        /// <summary>
        ///   /// <summary>
        /// Returns the version of the file that was processed last time.
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns all the files that are detected as inputs for processing.
        /// </summary>
        public FileWithDirectory[] InputFiles { get; set; }
        
        public IEnumerable<FileWithDirectory> GetChangedInputFiles()
        {
            if (InputFiles == null)
            {
                yield break;
            }
            else
            {
                foreach (var item in InputFiles)
                {
                    if (IsDifferentFromLastTime(item))
                    {
                        yield return item;
                    }
                }
            }
           
        }

        public bool HasChanges { get; private set; }

        public void SetInputFiles(IFileProvider fileProvider)
        {
            SetInput(fileProvider.GetFiles(this.Input));
            // ExcludeFiles = FileProvider.GetFiles(input.e)
        }

        private void SetInput(FileWithDirectory[] input)
        {
            // grab the input files from the file provider.
            PreviousInputFiles = InputFiles;
            InputFiles = input;
            HasChanges = false;
            var changedInputs = new List<FileWithDirectory>(InputFiles.Length);
            foreach (var item in InputFiles)
            {
                if (IsDifferentFromLastTime(item))
                {
                    HasChanges = true;
                    break;
                }
            }

        }
        
    }
}