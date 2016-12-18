using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.FileProviders;
using NetPack.Pipeline;

namespace NetPack.Pipes
{
    public class UglifyMinifyJsPipe : IPipe
    {
        public async Task ProcessAsync(IPipelineContext context, CancellationToken cancelationToken)
        {
            // Need to run uglify on any .js, .css, .html, or .htm files passed through the pipeline.
            foreach (var inputFile in context.InputFiles)
            {
                // only interested in typescript files.
                var inputFileInfo = inputFile.FileInfo;
                MinifyJsFile(context, inputFileInfo);

                //var ext = System.IO.Path.GetExtension(inputFileInfo.Name).ToLowerInvariant();
                //switch (ext)
                //{
                //    case ".css":
                //        MinifyCssFile(context, inputFileInfo);
                //        break;
                //    case ".html":
                //    case ".htm":
                       
                //        break;
                //    case ".js":
                //        MinifyJsFile(context, inputFileInfo);
                //        break;
                //    default:
                //        // allow other file types to pass through untouched.
                //     //   context.AddOutput(inputFile);
                //        break;

                //}
            }
        }
        
        private void MinifyJsFile(IPipelineContext context, IFileInfo inputFileInfo)
        {
            throw new NotImplementedException();

        }

    
    }
}

