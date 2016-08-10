using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Utils;
using Newtonsoft.Json.Linq;
using NUglify.JavaScript.Syntax;

namespace NetPack.Pipes
{
    public class CombinePipe : IPipe
    {

        private JObject _sourceMap;
        private CombinePipeOptions _options;

        public CombinePipe() : this(new CombinePipeOptions())
        {

        }

        public CombinePipe(CombinePipeOptions options)
        {
            _options = options;
        }

        public async Task ProcessAsync(IPipelineContext context, CancellationToken cancelationToken)
        {

        }

        /// <summary>
        /// Combines files into a single stream
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        private async Task<MemoryStream> GetCombinedStreamAsync(IFileProvider fileProvider, IEnumerable<string> filePaths)
        {

            var scriptInfos = new List<ScriptInfo>();

            var ms = new MemoryStream();
            int totalLineCount = 0;

            var combiner = new ScriptCombiner();

            foreach (var filePath in filePaths)
            {
                var fileInfo = fileProvider.GetFileInfo(filePath);

                if (fileInfo.Exists && !fileInfo.IsDirectory)
                {
                    using (var sourceFileStream = fileInfo.CreateReadStream())
                    {
                        // await fileStream.CopyToAsync(ms);

                        ScriptInfo sourceScript;
                        using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                        {
                            sourceScript = combiner.AddScript(sourceFileStream, writer);
                            scriptInfos.Add(sourceScript);

                            totalLineCount = totalLineCount + sourceScript.LineCount;

                            //if (sourceScript.SourceMapDeclaration != null)
                            //{
                            //    // remove the source mapping url declaration line from the bundle.
                            //    writer.BaseStream.Position = sourceScript.SourceMapDeclaration.Position;
                            //    writer.WriteLine();
                            //    // we need to remove the declartion line from the concatenated output
                            //    // and instead, output a new source map that represents where the sources
                            //    // ar ein the bundle file.

                            //}
                        }


                    }
                }
            }
            //ensure it's reset
            ms.Position = 0;
            return ms;
        }





    }

    // StreamUtil.cs:
}