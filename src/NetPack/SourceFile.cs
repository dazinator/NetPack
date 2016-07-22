using Microsoft.Extensions.FileProviders;

namespace NetPack
{
    public class SourceFile
    {
        public SourceFile(IFileInfo fileInfo, string directory)
        {
            Directory = directory;
            FileInfo = fileInfo;
        }

        public IFileInfo FileInfo { get; }

        public string Directory { get; }
    }
}