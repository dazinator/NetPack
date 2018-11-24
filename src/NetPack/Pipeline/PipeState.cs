using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;

namespace NetPack.Pipeline
{
    public class PipeState
    {

        public PipeState(FileWithDirectory[] inputs, IFileProvider fileProvider)
        {
            InputFiles = inputs;
            FileProvider = fileProvider;
            OutputFiles = new HashSet<Tuple<PathString, IFileInfo>>();
            SourceFiles = new HashSet<Tuple<PathString, IFileInfo>>();            
        }

        public FileWithDirectory[] InputFiles { get; set; }
        public IFileProvider FileProvider { get; }
        public HashSet<Tuple<PathString, IFileInfo>> OutputFiles { get; set; }

        public HashSet<Tuple<PathString, IFileInfo>> SourceFiles { get; set; }

        /// <summary>
        ///   /// <summary>
        /// Returns the specified input file.
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        public FileWithDirectory GetInputFile(FileWithDirectory fileWithDirectory)
        {
            if (InputFiles == null)
            {
                return null;
            }

            foreach (FileWithDirectory item in InputFiles)
            {
                if (item.UrlPath == fileWithDirectory.UrlPath)
                {
                    return item;
                }
            }

            return null;
        }              


        public bool HasChanged(PipeState previousState)
        {
            if (InputFiles == null)
            {
                return true;
                // yield break;
            }
            else
            {

                foreach (FileWithDirectory item in InputFiles)
                {
                    FileWithDirectory oldFile = previousState.GetInputFile(item);
                    if (item.IsNewerThan(oldFile))
                    {
                        return true;
                        // yield return item;
                    }
                }
            }

            return false;
        }

        public IEnumerable<FileWithDirectory> GetModifiedInputs(PipeState previousState)
        {
            if (InputFiles == null)
            {
               // return true;
                yield break;
            }
            else
            {

                foreach (FileWithDirectory item in InputFiles)
                {
                    FileWithDirectory oldFile = previousState.GetInputFile(item);
                    if (item.IsNewerThan(oldFile))
                    {                       
                        yield return item;
                    }
                }
            }

            yield break;
        }

        public void AddOutput(PathString directory, IFileInfo file)
        {
            // var item = new FileWithDirectory() { Directory = directory, FileInfo = fileInfo };
            Tuple<PathString, IFileInfo> pathAndFile = Tuple.Create<PathString, IFileInfo>(directory, file);
            OutputFiles.Add(pathAndFile);
        }

        /// <summary>
        /// Source files added as output do not trigger any onward processing, but can be served up my netpack's IFileProvider.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileInfo"></param>
        public void AddSource(PathString directory, IFileInfo file)
        {
            Tuple<PathString, IFileInfo> pathAndFile = Tuple.Create<PathString, IFileInfo>(directory, file);
            SourceFiles.Add(pathAndFile);
        }
    }
}