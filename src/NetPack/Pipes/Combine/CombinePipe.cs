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
using Microsoft.AspNetCore.Http;

namespace NetPack.Pipes
{
    public class JsCombinePipe : IPipe
    {

        private JObject _sourceMap;
        private JsCombinePipeOptions _options;

        private Dictionary<string, FileWithDirectory> _lastProcessed = new Dictionary<string, FileWithDirectory>();

        public JsCombinePipe() : this(new JsCombinePipeOptions())
        {

        }

        public JsCombinePipe(JsCombinePipeOptions options)
        {
            _options = options;
        }

        public async Task ProcessAsync(IPipelineContext context, CancellationToken cancelationToken)
        {
            bool hasChanges = false;

            string outputSubPath;
            if (!_options.OutputFilePath.StartsWith("/"))
            {
                outputSubPath = "/" + _options.OutputFilePath;
            }
            else
            {
                outputSubPath = _options.OutputFilePath;
            }

            foreach (var item in context.InputFiles)
            {

                if (item.FileSubPath == outputSubPath)
                {
                    continue;
                }
                if (context.IsDifferentFromLastTime(item))
                {
                    hasChanges = true;
                }
            }

            if (!hasChanges)
            {
                return;
            }

            bool hasSourceMappingDirectives = false;
            var combiner = new ScriptCombiner();
            var scriptInfos = new List<CombinedScriptInfo>(context.InputFiles.Length);

            var ms = new MemoryStream();

            int totalLineCount = 0;
            var encoding = Encoding.UTF8;

            foreach (var fileWithDirectory in context.InputFiles)
            {
                var fileInfo = fileWithDirectory.FileInfo;
                if (fileInfo.Exists && !fileInfo.IsDirectory)
                {
                    using (var sourceFileStream = fileInfo.CreateReadStream())
                    {
                        // await fileStream.CopyToAsync(ms);
                        using (var writer = new StreamWriter(ms, encoding, 1024, true))
                        {
                            var sourceScript = await combiner.AddScript(sourceFileStream, writer);
                            sourceScript.LineNumberOffset = totalLineCount;
                            sourceScript.FileWithDirectory = fileWithDirectory;
                            //  sourceScript.Path = fileInfo.Name;
                            scriptInfos.Add(sourceScript);
                            hasSourceMappingDirectives = hasSourceMappingDirectives | sourceScript.SourceMapDeclaration != null;
                            totalLineCount = totalLineCount + sourceScript.LineCount;
                        }
                    }
                }
            }

            //   var outputFilep = FileWithDirectory.Parse(_options.OutputFilePath);

            // Now if there are source mapping url directives present, need to produce a new source map file and directive.
            var outputFilePath = SubPathInfo.Parse(_options.OutputFilePath);
            if (hasSourceMappingDirectives && _options.SourceMapMode != SourceMapMode.None)
            {

                // we are creating a new source map file for the new combined file.
                // it will have the same name but ".map" appended.


                //var mapFilePath = context.GetServePath(outputFilePath.ToString() + ".map");
                //  var mapFileWithDirectory = new FileWithDirectory() { Directory = }
                // SubPathInfo.Parse(context.BaseRequestPath + "/" + outputFilePath.ToString() + ".map");
                var indexMapFile = BuildIndexMap(ms, scriptInfos, outputFilePath, context);

                // Output the new map file in the pipeline.
                context.AddOutput(outputFilePath.Directory, indexMapFile);

                // sourcemapping url is resolved relative to the source ifle
                var mapServePath = $"{indexMapFile.Name}"; // context.GetRequestPath(outputFilePath.Directory, indexMapFile);
                                                           //  var mapServePath = context.GetRequestPath(outputFilePath.Directory, indexMapFile);
                                                           // 4. Write a SourceMappingUrl pointing to the new map file subpath, to the end of the combined file (memory stream)
                using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                {
                    writer.WriteLine();
                    await writer.WriteLineAsync($"//# sourceMappingURL={mapServePath.ToString()}");
                }

                // make sure all the source js files can be served up to the browser.
                foreach (var item in scriptInfos)
                {
                    context.AddSourceOutput(item.FileWithDirectory.Directory, item.FileWithDirectory.FileInfo);
                }


            }

            //ensure it's reset
            ms.Position = 0;
            var bundleJsFile = new MemoryStreamFileInfo(ms, encoding, outputFilePath.Name);
            // Output the new combines file.
            context.AddOutput(outputFilePath.Directory, bundleJsFile);
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


        private IFileInfo BuildIndexMap(MemoryStream ms, List<CombinedScriptInfo> scriptInfos, SubPathInfo combinedFilePath, IPipelineContext context)
        {
            // todo
            // throw new NotImplementedException();



            // 1. Create a new index map json object, and append sections for each of the existing source map declarations.
            JObject indexMap = new JObject();
            indexMap["version"] = 3;
            indexMap["file"] = combinedFilePath.Name;

            JArray sections = new JArray();

            // for each of the original scripts that was combined.
            // check for a source map declaration - which is a requestpath to a .map file.
            var mapFileName = combinedFilePath.ToString() + ".map";
            foreach (var script in scriptInfos)
            {
                var declaration = script.SourceMapDeclaration;
                if (declaration != null)
                {
                    JObject sectionObject = new JObject();
                    sectionObject["offset"] = new JObject();
                    sectionObject["offset"]["line"] = script.LineNumberOffset;
                    sectionObject["offset"]["column"] = 0;

                    bool isAbsoluteUri = false;
                    if (declaration.SourceMappingUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        declaration.SourceMappingUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        isAbsoluteUri = true;
                    }

                    if (isAbsoluteUri)
                    {
                        // we need to find the local source map file to inline it.
                        throw new NotImplementedException("Combine pipe cannot combine source maps for files that have sourcemaps with absolute urls.");
                    }

                    // ok so the referenced source map is in a relative location.
                    // we need to be able to resolve the source map.

                    // The map file should live in path relative to its source file.
                    // if it already starts with a / then its relative to site root allready.
                    string sourceMapFileSubPath;
                    if (!declaration.SourceMappingUrl.StartsWith("/"))
                    {
                        sourceMapFileSubPath = $"{script.FileWithDirectory.Directory}/{declaration.SourceMappingUrl}";
                    }
                    else
                    {
                        sourceMapFileSubPath = declaration.SourceMappingUrl;
                    }

                    var sourceMapFile = context.FileProvider.GetFileInfo(sourceMapFileSubPath);

                    if (sourceMapFile == null || !sourceMapFile.Exists || sourceMapFile.IsDirectory)
                    {
                        throw new FileNotFoundException("Could not find the specified source map file which is referenced by a script", sourceMapFileSubPath);
                    }

                    // read the contents of the source map file, and inline it into the new source map file.
                    JObject sourceMapObject = null;
                    var sourceMapFileContents = sourceMapFile.ReadAllContent();
                    sourceMapObject = JObject.Parse(sourceMapFileContents);




                    AdjustSourceMapPathsRelativeToSiteRoot(sourceMapObject, combinedFilePath.Directory, script, context);

                    // if we couldn't find the source map file, then it means the source mapping url declaration in the 
                    // js file that we processed, does not take a form that identifies a file with the IFileProvider.

                    if (sourceMapObject == null)
                    {
                        // if its relative, then treat it as relative to the original script location.
                        // var mappingUrl = SubPathInfo.Parse(declaration.SourceMappingUrl);
                        throw new NotImplementedException();
                        //  var relativePathFromCombinedFileToScriptFile = combinedFilePath.GetRelativePathTo(script.Path);
                        //  var sourceMapSubPath = SubPathInfo.Parse(script.Path.Directory + "/" + sourceMapFilePath.Name);

                        //  var url = "/" + sourceMapSubPath;
                        //  sectionObject["url"] = url.ToString();

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

            var file = new StringFileInfo(indexMap.ToString(), mapFileName);
            return file;


            // An Index Map looks like this, as per the spec here: https://docs.google.com/document/d/1U1RGAehQwRypUTovF1KRlpiOFze0b-_2gc6fAH0KY0k/edit?pref=2&pli=1

            //            {
            //                  version: 3,
            //                  file: �app.js�,
            //                  sections: 
            //                    [
            //                      { offset: { line: 0, column: 0}, url: �url_for_part1.map� }
            //                      { offset: { line: 100, column: 10}, map:
            //                          {
            //                              version: 3,
            //                              file: �section.js�,
            //                              sources: ["foo.js", "bar.js"],
            //                              names: ["src", "maps", "are", "fun"],
            //                              mappings: "AAAA,E;;ABCDE;"
            //                          }
            //                      }
            //                    ],
            //             }

        }


        private void AdjustSourceMapPathsRelativeToSiteRoot(JObject sourceMapObject, string sourceMapDirectory, CombinedScriptInfo script, IPipelineContext context)
        {

            // because we are inlining this source map into our index source map,
            // the file / source file paths in the source map need to be adjuted to be relative to our index source map.

           // var sourceMapFilePath = $"{outputFileSubPath.Directory}/{sourceMapFileName}";
         //   var sourceMapSubPathInfo = SubPathInfo.Parse(sourceMapFilePath);

            // var sourceMapFilePathString = PathStringExtensions.Parse(sourceMapFilePath);
            // var scriptFilePathString = PathStringExtensions.Parse(script.FileWithDirectory.FileSubPath);            
            var relativeSubPath = SubpathHelper.MakeRelativeSubpath(sourceMapDirectory, script.FileWithDirectory.FileSubPath);

                //sourceMapFilePathString.MakeRelative(scriptFilePathString);

           // var reverseRelativePath = scriptFilePathString.MakeRelative(sourceMapFilePathString);

            //  var siteRootRelativeFilePath = context.GetRequestPath, script.FileWithDirectory.FileInfo);
            sourceMapObject["file"] = relativeSubPath.ToString();

            var sourcesArray = sourceMapObject["sources"] as JArray;
            if (sourcesArray != null)
            {
                for (int i = 0; i < sourcesArray.Count; i++)
                {
                    var item = sourcesArray[i];
                    var sourceFileSubPath = $"{script.FileWithDirectory.Directory}/{item.ToString()}";
                    var sourceFile = context.FileProvider.GetFileInfo(sourceFileSubPath);
                    if (!sourceFile.Exists)
                    {
                        // todo throw an exception because the sourcemap appears to be pointing to a file that doesnt exist.?
                        // ensure that source file is accessbile.
                        //TODO: Need a way to serve up sources without triggering a change.
                        //  if(!out)
                        throw new Exception("Source map refers to a file that could not be found: " + sourceFileSubPath);
                    }

                    // ensure the source file can be served up in the browser.
                    context.AddSourceOutput(script.FileWithDirectory.Directory, sourceFile);

                  //  var sourcesFilePathString = PathStringExtensions.Parse(sourceFileSubPath);
                    var relativePathToSourceFile = SubpathHelper.MakeRelativeSubpath(sourceMapDirectory, sourceFileSubPath);

                    //sourceMapFilePathString.MakeRelative(sourcesFilePathString);

                    sourcesArray[i] = relativePathToSourceFile.ToString();
                    //context.GetRequestPath(script.FileWithDirectory.Directory, sourceFile).ToString();
                }
            }


        }
    }

}