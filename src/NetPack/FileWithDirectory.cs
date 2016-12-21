using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetPack
{
    public class FileWithDirectory : IEquatable<FileWithDirectory>
    {
        public string Directory { get; set; }

        public string FileSubPath { get { return $"{Directory}/{FileInfo.Name}"; } }

        public IFileInfo FileInfo { get; set; }

        public bool Equals(FileWithDirectory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Directory, other.Directory) && FileInfo.Equals(other.FileInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileWithDirectory) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Directory.GetHashCode()*397) ^ FileInfo.GetHashCode();
            }
        }

      
    }
}
