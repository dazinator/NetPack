using System.IO;
using System.IO.Compression;

namespace NetPack.Zip.Tests
{
    public class ZipArchiveBuilder
    {
        private readonly ZipArchive _archive;

        public ZipArchiveBuilder(ZipArchive archive)
        {
            _archive = archive;
        }

        public void AddFile(string name, byte[] bytes)
        {
            ZipArchiveEntry zipEntry = _archive.CreateEntry(name);
            using (StreamWriter writer = new StreamWriter(zipEntry.Open()))
            {
                writer.BaseStream.Write(bytes);
                //writer.Write(bytes);
            }
        }
    }
}
