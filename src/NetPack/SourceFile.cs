using Microsoft.Extensions.FileProviders;

namespace NetPack
{
    public class SourceFile
    {

        public SourceFile(IFileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }

        public IFileInfo FileInfo { get; }

    }
}