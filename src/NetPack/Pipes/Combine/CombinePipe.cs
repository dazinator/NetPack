using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipeline;
using NetPack.Pipes.Combine;
using Newtonsoft.Json.Linq;

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

            var scriptInfos = new List<CombinedScriptInfo>();

            var ms = new MemoryStream();
            int totalLineCount = 0;

            var combiner = new ScriptCombiner();
            bool hasSourceMappingDirectives = false;

            foreach (var filePath in filePaths)
            {
                var fileInfo = fileProvider.GetFileInfo(filePath);

                if (fileInfo.Exists && !fileInfo.IsDirectory)
                {
                    using (var sourceFileStream = fileInfo.CreateReadStream())
                    {
                        // await fileStream.CopyToAsync(ms);

                        CombinedScriptInfo sourceScript;
                        using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                        {
                            sourceScript = combiner.AddScript(sourceFileStream, writer);
                            sourceScript.LineNumberOffset = totalLineCount;
                            scriptInfos.Add(sourceScript);
                            hasSourceMappingDirectives = hasSourceMappingDirectives | sourceScript.SourceMapDeclaration != null;
                            totalLineCount = totalLineCount + sourceScript.LineCount;
                        }
                    }
                }
            }

            // Now if there are source mapping url directives present, need to produce a new source map file and directive.
            if (!hasSourceMappingDirectives)
            {
                BuildSourceMap(ms, scriptInfos);
            }

            //ensure it's reset
            ms.Position = 0;
            return ms;

        }

        private void BuildSourceMap(MemoryStream ms, List<CombinedScriptInfo> scriptInfos)
        {
            // todo
            throw new NotImplementedException();

            // 1. Locate the existing source maps and read them into json object.


            // 2. Create a new index map json object, and append sections for each of the existing source maps.

            // 3. Output the sourcemap as a new in-memory file, from the pipeline, on the subpath /[BundileFilePath].map

            // 4. Write a SourceMappingUrl pointing to the new map file subpath, to the end of the combined file (memory stream)

            // An Index Map looks like this, as per the spec here: https://docs.google.com/document/d/1U1RGAehQwRypUTovF1KRlpiOFze0b-_2gc6fAH0KY0k/edit?pref=2&pli=1

            //            {
            //                  version: 3,
            //                  file: “app.js”,
            //                  sections: 
            //                    [
            //                      { offset: { line: 0, column: 0}, url: “url_for_part1.map” }
            //                      { offset: { line: 100, column: 10}, map:
            //                          {
            //                              version: 3,
            //                              file: “section.js”,
            //                              sources: ["foo.js", "bar.js"],
            //                              names: ["src", "maps", "are", "fun"],
            //                              mappings: "AAAA,E;;ABCDE;"
            //                          }
            //                      }
            //                    ],
            //             }

        }
    }

    // StreamUtil.cs:
}