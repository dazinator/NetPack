using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using NetPack.Pipeline;
using NetPack.Pipes.Combine;
using Newtonsoft.Json.Linq;
using System.Linq;
using NetPack.Utils;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.FileInfo;

namespace NetPack.Pipes
{
    public class JsCombinePipe : IPipe
    {

        private JObject _sourceMap;
        private JsCombinePipeOptions _options;

        public JsCombinePipe() : this(new JsCombinePipeOptions())
        {

        }

        public JsCombinePipe(JsCombinePipeOptions options)
        {
            _options = options;
        }

        public async Task ProcessAsync(IPipelineContext context, IFileInfo[] input, CancellationToken cancelationToken)
        {
            CombineJs(context, input, cancelationToken);
        }

        //private bool IsCssFile(SourceFile sourceFile)
        //{
        //    if (sourceFile.FileInfo != null && System.IO.Path.GetExtension(sourceFile.FileInfo.Name)
        //        .Equals(".css", StringComparison.OrdinalIgnoreCase))
        //    {
        //        _cssFiles.Add(sourceFile); // add the css file to our collection for later.
        //        return true;
        //    }
        //    return false;
        //}

        private void CombineJs(IPipelineContext context, IEnumerable<IFileInfo> jsFiles, CancellationToken cancelationToken)
        {

            bool hasSourceMappingDirectives = false;
            var combiner = new ScriptCombiner();
            var scriptInfos = new List<CombinedScriptInfo>();
            var ms = new MemoryStream();
            int totalLineCount = 0;
            var encoding = Encoding.UTF8;

            foreach (var fileInfo in jsFiles)
            {
                if (fileInfo.Exists && !fileInfo.IsDirectory)
                {
                    using (var sourceFileStream = fileInfo.CreateReadStream())
                    {
                        // await fileStream.CopyToAsync(ms);
                        using (var writer = new StreamWriter(ms, encoding, 1024, true))
                        {
                            var sourceScript = combiner.AddScript(sourceFileStream, writer);
                            sourceScript.LineNumberOffset = totalLineCount;
                            sourceScript.Path = fileInfo.Name;
                            scriptInfos.Add(sourceScript);
                            hasSourceMappingDirectives = hasSourceMappingDirectives | sourceScript.SourceMapDeclaration != null;
                            totalLineCount = totalLineCount + sourceScript.LineCount;
                        }
                    }
                }
            }


            // Now if there are source mapping url directives present, need to produce a new source map file and directive.
            var outputFilePath = SubPathInfo.Parse(_options.CombinedJsFileName);
            if (hasSourceMappingDirectives && _options.EnableIndexSourceMap)
            {


                var mapFilePath = SubPathInfo.Parse(context.BaseRequestPath + "/" + outputFilePath.ToString() + ".map");
                var indexMapFile = BuildIndexMap(ms, scriptInfos, mapFilePath, outputFilePath, context);

                // Output the new map file in the pipeline.
                //TODO: Add this line back..
                context.AddOutput(new SourceFile(indexMapFile, outputFilePath.Directory));

                // 4. Write a SourceMappingUrl pointing to the new map file subpath, to the end of the combined file (memory stream)
                using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                {
                    writer.WriteLine();
                    writer.WriteLine($"//# sourceMappingURL=/{mapFilePath.ToString()}");
                }
            }

            //ensure it's reset
            ms.Position = 0;
            var bundleJsFile = new MemoryStreamFileInfo(ms, encoding, outputFilePath.Name);
            // Output the new map file in the pipeline.
            context.AddOutput(new SourceFile(bundleJsFile, outputFilePath.Directory));

        }

        private IFileInfo BuildIndexMap(MemoryStream ms, List<CombinedScriptInfo> scriptInfos, SubPathInfo mapFilePath, SubPathInfo combinedFilePath, IPipelineContext context)
        {
            // todo
            // throw new NotImplementedException();

            // 1. Create a new index map json object, and append sections for each of the existing source map declarations.
            JObject indexMap = new JObject();
            indexMap["version"] = 3;
            indexMap["file"] = combinedFilePath.Name;

            JArray sections = new JArray();

            foreach (var script in scriptInfos)
            {
                var declaration = script.SourceMapDeclaration;
                if (declaration != null)
                {
                    JObject sectionObject = new JObject();
                    sectionObject["offset"] = new JObject();
                    sectionObject["offset"]["line"] = script.LineNumberOffset;
                    sectionObject["offset"]["column"] = 0;


                    //TODO: if the source mapping url is absolute should leave it as is.

                    // use the baserequest path + the souremapping url to get the full request path
                    var sourceMapFilePath = SubPathInfo.Parse(declaration.SourceMappingUrl);
                    var sourceMapFile = context.FileProvider.GetFileInfo(sourceMapFilePath.ToString());
                    JObject sourceMapObject = null;
                    if (sourceMapFile != null &&  !sourceMapFile.IsDirectory && sourceMapFile.Exists)
                    {
                        var sourceMapFileContents = sourceMapFile.ReadAllContent();
                        sourceMapObject = JObject.Parse(sourceMapFileContents);
                    }


                    if (sourceMapObject == null)
                    {
                        // if its relative, then treat it as relative to the original script location.
                        var mappingUrl = SubPathInfo.Parse(declaration.SourceMappingUrl);
                        var relativePathFromCombinedFileToScriptFile = combinedFilePath.GetRelativePathTo(script.Path);
                        var sourceMapSubPath = SubPathInfo.Parse(script.Path.Directory + "/" + mappingUrl.Name);
                        //SubPathInfo.Parse(relativePathFromCombinedFileToScriptFile.Directory + "/" + declaration.SourceMappingUrl);
                        // var relativePathToMappingUrl = relativePathFromCombinedFileToScriptFile.GetRelativePathTo(mappingUrl);
                        var url = "/" + sourceMapSubPath;
                        sectionObject["url"] = url.ToString();
                        // TODO: Inline the source maps rather than using url,
                        // so rather than appending a url here, could actually append the contents
                        // of the map file using map: { }
                        // var sourceMapFile = fileProvider.GetFileInfo(originalMapPath);
                    }
                    else
                    {
                        sectionObject["map"] = sourceMapObject;
                    }
                   

                    //  
                    sections.Add(sectionObject);
                }
            }

            indexMap["sections"] = sections;
            var file = new StringFileInfo(indexMap.ToString(), mapFilePath.Name);
            return file;


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

}