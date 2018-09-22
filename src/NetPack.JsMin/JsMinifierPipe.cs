using Dazinator.AspNet.Extensions.FileProviders;
using DotNet.SourceMaps;
using NetPack.Pipeline;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetPack.JsMin
{
    public class JsMinifierPipe : BasePipe
    {

        private JsMinOptions _options;

        public JsMinifierPipe(JsMinOptions options)
        {
            _options = options;

        }

        public override async Task ProcessAsync(PipeContext context, CancellationToken cancelationToken)
        {


            JsMin jsMin = new JsMin(_options);
            foreach (FileWithDirectory item in context.InputFiles)
            {

                string outPutFileName = GetOutputFileName(item);
                string subPath = item.Directory + outPutFileName;
                context.Blocker.AddBlock(subPath);

                // preemptive block any map file until we have processed.
                string mapFileName = outPutFileName + ".map";
                context.Blocker.AddBlock(subPath + ".map");

                SourceMapBuilder mapBuilder = GetSourceMapBuilder(_options, outPutFileName, context.PipelineContext, item);

                System.IO.Stream stream = item.FileInfo.CreateReadStream();
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                StringBuilder output = new StringBuilder((int)stream.Length); // minified file shouldnt be longer than the original
                                                                              //   mapBuilder.CurrentSourceFileContext.AdvanceColumnPosition(-1);

                await jsMin.ProcessAsync(stream, output, mapBuilder, cancelationToken);

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


                        context.PipelineContext.AddGeneratedOutput(item.Directory, new StringFileInfo(jsonSourceMap, mapFileName));
                        sourceMappingURL = $"//# sourceMappingURL={mapFileName.ToString()}";
                    }

                    output.Append(sourceMappingURL);
                }


                context.PipelineContext.AddGeneratedOutput(item.Directory, new StringFileInfo(output.ToString(), outPutFileName));

            }


        }

        private string GetJson(SourceMap sourceMap)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            string json = JsonConvert.SerializeObject(sourceMap, settings);
            return json;
        }

        private SourceMapBuilder GetSourceMapBuilder(JsMinOptions options, string outputFileName, IPipelineContext context, FileWithDirectory item)
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
                context.AddSourceOutput(item.Directory, item.FileInfo);


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

