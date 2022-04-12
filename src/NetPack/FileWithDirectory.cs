using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;

namespace NetPack
{
    public class FileWithDirectory : IEquatable<FileWithDirectory>
    {
        public PathString Directory { get; set; }

        public PathString UrlPath { get { return Directory.Add($"/{FileInfo.Name}"); } }

        public IFileInfo FileInfo { get; set; }

        public bool Equals(FileWithDirectory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Directory.Equals(other.Directory) && FileInfo.Equals(other.FileInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileWithDirectory)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Directory.GetHashCode() * 397) ^ FileInfo.GetHashCode();
            }
        }

        public bool IsNewerThan(FileWithDirectory file)
        {
            if (file == null)
            {
                // We don't have a previous version of this file, so the file must be a new version.
                return true;
            }

            // Return whether this version of the file has a gretaer modified date than the last version of this file.
            bool changed = FileInfo.LastModified > file.FileInfo.LastModified;
            return changed;
        }


    }
}
