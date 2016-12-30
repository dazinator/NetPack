using System;
using System.Threading;
using System.Threading.Tasks;
using NetPack.Pipeline;
using Dazinator.AspNet.Extensions.FileProviders;
using DotNet.SourceMaps;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NetPack.JsMin
{



    public class JsMinifierPipe : IPipe
    {

        private JsMinOptions _options;

        public JsMinifierPipe(JsMinOptions options)
        {
            _options = options;

        }

        public async Task ProcessAsync(IPipelineContext context, CancellationToken cancelationToken)
        {
            var jsMin = new JsMin(_options);
            foreach (var item in context.InputFiles)
            {

                string outPutFileName = GetOutputFileName(item);
                var mapBuilder = GetSourceMapBuilder(_options, outPutFileName, context, item);
                
                var stream = item.FileInfo.CreateReadStream();
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var output = new StringBuilder((int)stream.Length); // minified file shouldnt be longer than the original
                await jsMin.ProcessAsync(stream, output, mapBuilder, cancelationToken);

                if (mapBuilder != null)
                {
                    var sourceMap = mapBuilder.Build();
                    string jsonSourceMap = GetJson(sourceMap);

                    // either inline the source map, or output it as a seperate file.
                    output.AppendLine();
                    string sourceMappingURL;
                    if (_options.InlineSourceMap)
                    {
                        //@ sourceMappingURL=data:application/json;charset=utf-8;base64,jadhadwkfa                      
                        var base64EncodedSourceMap = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonSourceMap));
                        sourceMappingURL = $"//# sourceMappingURL=data:application/json;charset=utf-8;base64,{base64EncodedSourceMap}";
                      
                    }
                    else
                    {
                        // seperate file.
                        var mapFileName = outPutFileName + ".map";
                        context.AddOutput(item.Directory, new StringFileInfo(jsonSourceMap, mapFileName));
                        sourceMappingURL = $"//# sourceMappingURL={mapFileName.ToString()}";                      
                    }

                    output.Append(sourceMappingURL);
                }

                context.AddOutput(item.Directory, new StringFileInfo(output.ToString(), outPutFileName));               

            }

        }

        private string GetJson(SourceMap sourceMap)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var json = JsonConvert.SerializeObject(sourceMap, settings);
            return json;
        }

        private SourceMapBuilder GetSourceMapBuilder(JsMinOptions options, string outputFileName, IPipelineContext context, FileWithDirectory item)
        {
            if (!options.EnableSourceMaps)
            {
                return null;
            }

            var mapBuilder = new SourceMapBuilder();
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
            var name = item.FileInfo.Name;
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

