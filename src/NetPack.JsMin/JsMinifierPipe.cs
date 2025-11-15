using DotNet.SourceMaps;
using NetPack.Pipeline;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.JsMin
{
    public class JsMinifierPipe : BasePipe
    {

        private JsMinOptions _options;

        public JsMinifierPipe(JsMinOptions options, string name = "JS Min") : base(name)
        {
            _options = options;

        }

        public override async Task ProcessAsync(PipeState state, CancellationToken cancelationToken)
        {
            JsMin jsMin = new JsMin(_options);
            var inputFiles = state.GetInputFiles();
            foreach (FileWithDirectory item in inputFiles)
            {

                string outPutFileName = GetOutputFileName(item);
                string subPath = item.Directory + outPutFileName;
               // state.AddBlock(subPath);

                // preemptive block any map file until we have processed.
                string mapFileName = outPutFileName + ".map";
               // state.AddBlock(subPath + ".map");

                SourceMapBuilder mapBuilder = GetSourceMapBuilder(_options, outPutFileName, state, item);

                System.IO.Stream stream = item.FileInfo.CreateReadStream();
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                StringBuilder output = new StringBuilder((int)stream.Length); // minified file shouldnt be longer than the original
                                                                              //   mapBuilder.CurrentSourceFileContext.AdvanceColumnPosition(-1);

                cancelationToken.ThrowIfCancellationRequested();
                await jsMin.ProcessAsync(stream, output, mapBuilder, cancelationToken);
                cancelationToken.ThrowIfCancellationRequested();

                if (mapBuilder != null)
                {
                    SourceMap sourceMap = mapBuilder.Build();
                    string jsonSourceMap = GetJson(sourceMap);

                    // either inline the source map, or output it as a seperate file.
                    output.AppendLine();
                    string sourceMappingURL;
                    if (_options.InlineSourceMap)
                    {
                        //@ sourceMappingURL=data:application/json;charset=utf-8;base64,jadhadwkfa                      
                        string base64EncodedSourceMap = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonSourceMap));
                        sourceMappingURL = $"//# sourceMappingURL=data:application/json;charset=utf-8;base64,{base64EncodedSourceMap}";

                    }
                    else
                    {
                        // seperate file.

                        state.AddStringFile(item.Directory, jsonSourceMap, mapFileName);
                        sourceMappingURL = $"//# sourceMappingURL={mapFileName.ToString()}";
                    }

                    output.Append(sourceMappingURL);
                }

                state.AddStringFile(item.Directory, output.ToString(), outPutFileName);

            }


        }

        private string GetJson(SourceMap sourceMap)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string json = JsonSerializer.Serialize(sourceMap, options);
            return json;
        }

        private SourceMapBuilder GetSourceMapBuilder(JsMinOptions options, string outputFileName, PipeState state, FileWithDirectory item)
        {
            if (!options.EnableSourceMaps)
            {
                return null;
            }

            SourceMapBuilder mapBuilder = new SourceMapBuilder();
            mapBuilder.WithOutputFile(outputFileName);

            if (_options.InlineSources)
            {
                mapBuilder.WithSource(item.FileInfo.ReadAllContent(), true);
            }
            else
            {
                // make sure the source file can be served up to the browser.
                state.AddSource(item.Directory, item.FileInfo);


                // Not sure if its necessary to create a relative path from min or map file to the source file:
                //  var relativePathToSourceFile = SubpathHelper.MakeRelativeSubpath(sourceMapDirectory, sourceFileSubPath);
                // For now, just referencing source file by name, as its assumed the source file fill be along side the miniied file, so 
                // a relative path thats just the source file name should hopefully be resolved ok.
                mapBuilder.WithSource(item.FileInfo.Name);
            }


            return mapBuilder;


        }



        private string GetOutputFileName(FileWithDirectory item)
        {
            string name = item.FileInfo.Name;
            if (name.EndsWith(".js"))
            {
                name = item.FileInfo.Name.Substring(0, name.Length - 3) + ".min.js";
            }
            else
            {
                name = name + "min.js";
            }

            return name;
        }
    }
}

