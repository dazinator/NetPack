using System;
using System.Collections.Generic;
using NetPack.File;
using System.Linq;

namespace NetPack.Pipeline
{
    public class PipelineContext : IPipelineContext
    {

        public PipelineContext() : this(new List<SourceFile>())
        {
        }


        public PipelineContext(List<SourceFile> inputFiles)
        {
            OutputFiles = new List<SourceFile>();
            InputFiles = inputFiles;
        }

        public SourceFile[] Input => InputFiles.ToArray();

        public void AddOutput(SourceFile info)
        {
            OutputFiles.Add(info);
        }

        /// <summary>
        /// Causes all files dont match the filter to added as outputs, and those match, to be returned.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public SourceFile[] ApplyFilter(Predicate<SourceFile> filter)
        {
            var matches = new List<SourceFile>();
            foreach (var file in Input)
            {
                if (!filter(file))
                {
                    AddOutput(file);
                    continue;
                }
                matches.Add(file);
            }

            return matches.ToArray();
        }

        public SourceFile[] GetFilesByExtension(string fileExtensionIncludingDotPrefix)
        {
            var files =
                    Input.Where(
                        a => a.FileInfo != null && a.FileInfo.Name.EndsWith(fileExtensionIncludingDotPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
            return files;
        }

        public List<SourceFile> OutputFiles { get; set; }

        public List<SourceFile> InputFiles { get; set; }


        public void PrepareNextInputs()
        {
            // Sets the inputs ready for the next pipe,
            // by grabbing them from the outputs of previous pipe.
            InputFiles = OutputFiles;
            OutputFiles = new List<SourceFile>();
        }
    }
}