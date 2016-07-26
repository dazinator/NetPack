using Microsoft.Extensions.FileProviders;

namespace NetPack.File
{
    public class SourceFile
    {
        public SourceFile(IFileInfo fileInfo, string directory)
        {
            Directory = directory;
            FileInfo = fileInfo;
        }

        public IFileInfo FileInfo { get; protected set; }

        public string Directory { get; }

        public string GetPath()
        {
            return $"{Directory}/{FileInfo.Name}";
        }

        public void Update(IFileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }

    }
}