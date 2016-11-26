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

namespace NetPack.Pipes
{
    public class CombinePipe : IPipe
    {

        private JObject _sourceMap;
        private CombinePipeOptions _options;

        private List<SourceFile> _jsFiles = new List<SourceFile>();
        private List<SourceFile> _cssFiles = new List<SourceFile>();


        public CombinePipe() : this(new CombinePipeOptions())
        {

        }

        public CombinePipe(CombinePipeOptions options)
        {
            _options = options;
        }

        public async Task ProcessAsync(IPipelineContext context, CancellationToken cancelationToken)
        {
            Predicate<SourceFile> filter = (a) => false;

            if (_options.EnableJavascriptBundle)
            {
                filter = IsJsFile;
            }
            if (_options.EnableCssBundle)
            {
                filter = PredicateHelper.Or<SourceFile>(filter, IsCssFile);
            }

            // ApplyFilter causes filters to evaluate the input files to this pipe,
            // and all the input files that don't match the filters, 
            // will be added to the output files of the pipe (pipelinecontext) untouched.
            // Those that do match are returned.
            // We only want js and / or css files based on whether we are configured to do js / css
            // combining.
            var candidateFiles = context.ApplyFilter(filter);
            if (_options.EnableJavascriptBundle)
            {
                CombineJs(context, _jsFiles, cancelationToken);
            }
            if (_options.EnableCssBundle)
            {
                CombineJs(context, _cssFiles, cancelationToken);
            }

        }

        private bool IsCssFile(SourceFile sourceFile)
        {
            if (sourceFile.FileInfo != null && System.IO.Path.GetExtension(sourceFile.FileInfo.Name)
                .Equals(".css", StringComparison.OrdinalIgnoreCase))
            {
                _cssFiles.Add(sourceFile); // add the css file to our collection for later.
                return true;
            }
            return false;
        }

        private bool IsJsFile(SourceFile sourceFile)
        {
            if (sourceFile.FileInfo != null && System.IO.Path.GetExtension(sourceFile.FileInfo.Name)
              .Equals(".js", StringComparison.OrdinalIgnoreCase))
            {
                _jsFiles.Add(sourceFile); // add the js file to our collection for later.
                return true;
            }
            return false;
        }

        private void CombineCss(IEnumerable<SourceFile> cssFiles)
        {
            throw new NotImplementedException();
        }

        private void CombineJs(IPipelineContext context, IEnumerable<SourceFile> jsFiles, CancellationToken cancelationToken)
        {




            bool hasSourceMappingDirectives = false;
            var combiner = new ScriptCombiner();
            var scriptInfos = new List<CombinedScriptInfo>();
            var ms = new MemoryStream();
            int totalLineCount = 0;
            var encoding = Encoding.UTF8;

            foreach (var sourceFile in jsFiles)
            {
                var fileInfo = sourceFile.FileInfo;
                if (fileInfo.Exists && !fileInfo.IsDirectory)
                {
                    using (var sourceFileStream = fileInfo.CreateReadStream())
                    {
                        // await fileStream.CopyToAsync(ms);
                        using (var writer = new StreamWriter(ms, encoding, 1024, true))
                        {
                            var sourceScript = combiner.AddScript(sourceFileStream, writer);
                            sourceScript.LineNumberOffset = totalLineCount;
                            sourceScript.Path = sourceFile.ContentPathInfo;
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
                    var sourceMapFile = context.FindFile(sourceMapFilePath);
                    JObject sourceMapObject = null;
                    if (sourceMapFile != null)
                    {
                        var sourceMapFileContents = sourceMapFile.FileInfo.ReadAllContent();
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