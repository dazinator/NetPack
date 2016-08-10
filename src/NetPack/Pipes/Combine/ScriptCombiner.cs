using System.IO;

namespace NetPack.Pipes
{
    public class ScriptCombiner
    {
        public ScriptInfo AddScript(Stream sourceStream, StreamWriter writer)
        {
            var scriptInfo = new ScriptInfo();



            // read the stream backwards to check for
            // a # sourceMappingURL=<url>
            var reverseReader = new ReverseLineReader(() => sourceStream, true);
            int lineOffset = 0;

            foreach (var line in reverseReader)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    SourceMappingUrlDeclaration sourceMappingUrlDeclaration;
                    if (SourceMappingUrlDeclaration.TryParse(line, out sourceMappingUrlDeclaration))
                    {
                        scriptInfo.SourceMapDeclaration = sourceMappingUrlDeclaration;
                        //  scriptInfo.SourceMapDeclaration.LineNumber = ;
                        scriptInfo.SourceMapDeclaration.Position = reverseReader.CurrentLinePosition;
                    }
                    // no sourcemap url on last non empty line, dont check any more lines as it should be at the end.
                    break;

                }

                lineOffset = lineOffset + 1;
            }

              
            sourceStream.Seek(0, SeekOrigin.Begin);
            sourceStream.Position = 0;

            int lineCount = 0;


            using (var reader = new StreamReader(sourceStream))
            {
                reader.BaseStream.Position = 0;
                reader.DiscardBufferedData();

                if (scriptInfo.SourceMapDeclaration == null)
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        lineCount = lineCount + 1;
                        writer.WriteLine(line);
                    }
                    scriptInfo.LineCount = lineCount;
                    return scriptInfo;
                }

                // reader.BaseStream.Seek(0, SeekOrigin.Begin);
               

                while (!reader.EndOfStream)
                {

                    var startPos = reader.GetActualPosition();
                    var line = reader.ReadLine();
                    lineCount = lineCount + 1;
                    //var endPos = reader.GetActualPosition();

                    if (startPos > scriptInfo.SourceMapDeclaration.Position)
                    {
                        // this must be the line with the source map declaration so omit it.
                        scriptInfo.SourceMapDeclaration.LineNumber = lineCount;
                        writer.WriteLine();
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                }

                scriptInfo.LineCount = lineCount;
                return scriptInfo;
            }



        }
    }
}