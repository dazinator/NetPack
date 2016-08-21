using System.IO;
using Microsoft.Extensions.FileProviders;

namespace NetPack.File
{
    public class SourceFile
    {
        public SourceFile(IFileInfo fileInfo, string directory)
        {
            // Directory = directory;
            FileInfo = fileInfo;
            ContentPathInfo = SubPathInfo.Parse($"{directory}/{FileInfo.Name}");
           // Directory = SubPathInfo.Parse(directory);
        }

        public SubPathInfo ContentPathInfo { get; set; }

        public SubPathInfo WebRootPathInfo { get; set; }

        //public SubPathInfo Directory { get; set; }

        public IFileInfo FileInfo { get; protected set; }

        //  public string Directory { get; }

        public override string ToString()
        {
            return ContentPathInfo.ToString();
        }

        public void Update(IFileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }

    }
}