using System;
using System.IO;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;
using NetPack.File;

namespace NetPack
{

    public class FileProcessContext
    {

        private Lazy<string> _contents;


        public FileProcessContext(SourceFile sourceFile)
        {
            SourceFile = sourceFile;
            _contents = new Lazy<string>(ReadContents);
        }

        public SourceFile SourceFile { get; set; }

        public string FileContents => _contents.Value;

        public string ReadContents()
        {
            using (var stream = new StreamReader(SourceFile.FileInfo.CreateReadStream()))
            {
                return stream.ReadToEnd();
            }
        }

    }
}