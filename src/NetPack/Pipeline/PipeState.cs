using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dazinator.Extensions.FileProviders;
using NetPack.Utils;

namespace NetPack.Pipeline
{
    public class PipeState
    {
        public PipeState(FileWithDirectory[] inputs, IFileProvider fileProvider)
        {
            InputFiles = new Dictionary<PathString, Tuple<FileWithDirectory, DateTimeOffset>>();
            foreach (FileWithDirectory item in inputs)
            {
                InputFiles.Add(item.UrlPath,
                    new Tuple<FileWithDirectory, DateTimeOffset>(item, item.FileInfo.LastModified));
            }

            FileProvider = fileProvider;
            OutputFiles = new HashSet<Tuple<PathString, IFileInfo>>();
            SourceFiles = new HashSet<Tuple<PathString, IFileInfo>>();
        }

        public Dictionary<PathString, Tuple<FileWithDirectory, DateTimeOffset>> InputFiles { get; set; }
        public IFileProvider FileProvider { get; }
        public HashSet<Tuple<PathString, IFileInfo>> OutputFiles { get; set; }

        public HashSet<Tuple<PathString, IFileInfo>> SourceFiles { get; set; }

        public FileWithDirectory[] GetInputFiles()
        {
            return InputFiles.Values.Select(a => a.Item1).ToArray();
        }

        /// <summary>
        ///   /// <summary>
        /// Returns the specified input file.
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        /// </summary>
        /// <param name="fileWithDirectory"></param>
        /// <returns></returns>
        public FileWithDirectory GetInputFile(PathString path)
        {
            if (InputFiles == null)
            {
                return null;
            }

            bool fileInfo = InputFiles.TryGetValue(path, out Tuple<FileWithDirectory, DateTimeOffset> info);
            return info.Item1;
        }

        public bool HasFileChanged(PathString path, DateTimeOffset currentLastModifiedDate)
        {
            if (InputFiles == null)
            {
                return true;
            }

            if (!InputFiles.TryGetValue(path, out Tuple<FileWithDirectory, DateTimeOffset> info))
            {
                return true;
            }

            return info.Item2 < currentLastModifiedDate;
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
                var allKeys = InputFiles.Keys;
                foreach (var item in allKeys)
                {
                    var currentItem = InputFiles[item];
                    var hasChangedSinePrevious = previousState.HasFileChanged(item, currentItem.Item2);
                    if (hasChangedSinePrevious)
                    {
                        return true;
                        // yield return item;
                    }
                }

                //foreach (FileWithDirectory item in InputFiles)
                //{
                //    FileWithDirectory oldFile = previousState.GetInputFileLastModified(item.UrlPath);
                //    if (item.IsNewerThan(oldFile))
                //    {
                //        return true;
                //        // yield return item;
                //    }
                //}
            }

            return false;
        }

        public IEnumerable<FileWithDirectory> GetModifiedInputs(PipeState previousState)
        {
            if (InputFiles == null)
            {
                yield break;
            }
            else
            {
                var allKeys = InputFiles.Keys;
                foreach (var item in allKeys)
                {
                    var currentItem = InputFiles[item];
                    var hasChangedSinePrevious = previousState.HasFileChanged(item, currentItem.Item2);
                    if (hasChangedSinePrevious)
                    {
                        yield return currentItem.Item1;
                        // yield return item;
                    }
                }

                //foreach (FileWithDirectory item in InputFiles)
                //{
                //    FileWithDirectory oldFile = previousState.GetInputFileLastModified(item.UrlPath);
                //    if (item.IsNewerThan(oldFile))
                //    {
                //        return true;
                //        // yield return item;
                //    }
                //}
            }

            //if (InputFiles == null)
            //{
            //    // return true;
            //    yield break;
            //}
            //else
            //{

            //    foreach (FileWithDirectory item in InputFiles)
            //    {
            //        FileWithDirectory oldFile = previousState.GetInputFile(item);
            //        if (item.IsNewerThan(oldFile))
            //        {
            //            yield return item;
            //        }
            //    }
            //}

            yield break;
        }

        public void AddOutput(PathString directory, IFileInfo file)
        {
            // var item = new FileWithDirectory() { Directory = directory, FileInfo = fileInfo };
            Tuple<PathString, IFileInfo> pathAndFile = Tuple.Create<PathString, IFileInfo>(directory, file);
            OutputFiles.Add(pathAndFile);
        }

        public void AddStringFile(string directoryPath, string content, Encoding encoding = null)
        {
            var path = directoryPath.ToPathString();
            //  SubPathInfo.Parse(directoryPath);
            AddStringFile(path, content, encoding);
        }


        public void AddStringFile(PathString directoryPath, string content, Encoding encoding = null)
        {
            var subPathInfo = PathStringHelper.SplitPath(directoryPath);
            AddStringFile(subPathInfo.Directory, content, subPathInfo.FileName, encoding);
        }

        public void AddStringFile(string directoryPath, string content, string fileName, Encoding encoding = null)
        {
            AddOutput(directoryPath, new StringFileInfo(content, fileName, encoding));
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