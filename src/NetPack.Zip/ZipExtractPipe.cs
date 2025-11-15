using Microsoft.AspNetCore.Http;
using NetPack.Pipeline;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Dazinator.Extensions.FileProviders;
using NetPack.Utils;

namespace NetPack.Zip
{
    public class ZipExtractPipe : BasePipe
    {

        private ZipExtractPipeOptions _options;

        public ZipExtractPipe() : this(new ZipExtractPipeOptions())
        {

        }

        public ZipExtractPipe(ZipExtractPipeOptions options, string name = "Zip Extract") : base(name)
        {
            _options = options;
        }

        public override async Task ProcessAsync(PipeState state, CancellationToken cancelationToken)
        {

            //   PathString outputSubPath = _options.OutputFilePath.ToPathString();
            // Dazinator.AspNet.Extensions.FileProviders.Directory.IDirectory dir;
            // var baseDir = dir.GetOrAddFolder("unzipped");

            var inputFiles = state.GetInputFiles();

            foreach (FileWithDirectory fileWithDirectory in inputFiles)
            {
                var fileInfo = fileWithDirectory.FileInfo;

                try
                {
                    using (var readStream = fileInfo.CreateReadStream())
                    {
                        using (ZipArchive archive = new ZipArchive(readStream, ZipArchiveMode.Read))
                        {
                            await ExtractArchive(archive, state, cancelationToken);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }
                
            }

        }

        private async Task ExtractArchive(ZipArchive archive, PipeState state, CancellationToken cancelationToken)
        {
            foreach (var entry in archive.Entries)
            {
                var ms = new MemoryStream();
                var readStream = entry.Open();

                await readStream.CopyToAsync(ms, cancelationToken);

                var memoryStreamFile = new MemoryStreamFileInfo(ms,  entry.Name);
               
                var subPath = SubPathInfo.Parse(entry.FullName);
                var pathString = new PathString($"/{subPath.Directory}");
                state.AddOutput(pathString, memoryStreamFile);
            }
        }
    }
}