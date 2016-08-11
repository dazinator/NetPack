using System;
using System.IO;
using NetPack.Extensions;
using NetPack.Utils;

namespace NetPack.Pipes.Combine
{
    public class ScriptCombiner
    {
        public CombinedScriptInfo AddScript(Stream sourceStream, StreamWriter writer)
        {
            var scriptInfo = new CombinedScriptInfo();

            // read the stream backwards to parse the last non empty line for
            // a # sourceMappingURL=<url> and record its position in the stream.

            var reverseReader = new ReverseLineReader(() => sourceStream, true);
            int lineOffset = 0;
            long sourceMappingUrlPosition = 0;

            foreach (var line in reverseReader)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    SourceMappingUrlDeclaration sourceMappingUrlDeclaration;
                    if (SourceMappingUrlDeclaration.TryParse(line, out sourceMappingUrlDeclaration))
                    {
                        scriptInfo.SourceMapDeclaration = sourceMappingUrlDeclaration;
                        sourceMappingUrlPosition = reverseReader.Position;
                    }
                    // no sourcemap url on last non empty line, dont check any more lines as it should be at the end.
                    break;
                }

                lineOffset = lineOffset + 1;
            }


            // If no sourcemap to worry about, just copy script to combined file,
            // and capture number of lines written.
            int linesWritten;
            if (scriptInfo.SourceMapDeclaration == null)
            {
                CombineWithoutSourceMap(sourceStream, writer, out linesWritten);
                scriptInfo.LineCount = linesWritten;
                return scriptInfo;
            }


            // A source map is present, copy the script to the bundle but omit the sourcemappingurl.
            int lineNumberOfSourceMapUrl;
            CombineWithSourceMap(sourceMappingUrlPosition, sourceStream, writer, out linesWritten, out lineNumberOfSourceMapUrl);

            scriptInfo.LineCount = linesWritten;
            scriptInfo.SourceMapDeclaration.LineNumber = lineNumberOfSourceMapUrl;
            return scriptInfo;
        }

        private void CombineWithoutSourceMap(Stream sourceStream, StreamWriter writer, out int linesWritten)
        {
            int lineCount = 0;
            sourceStream.Position = 0;
            using (var reader = new StreamReader(sourceStream))
            {
                reader.BaseStream.Position = 0;
                // reader.DiscardBufferedData();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    lineCount = lineCount + 1;
                    writer.WriteLine(line);
                }
                linesWritten = lineCount;
            }

        }

        private void CombineWithSourceMap(long sourceMappingUrlPosition, Stream sourceStream, StreamWriter writer, out int linesWritten, out int sourceMappingUrlLineNumber)
        {
            int lineCount = 0;
            int lineNumberOfSourceMap = 0;

            using (var reader = new StreamReader(sourceStream))
            {
                sourceStream.Position = 0;
                // reader.DiscardBufferedData();
                int newLineBytes = reader.CurrentEncoding.GetByteCount(Environment.NewLine);
                // reader.BaseStream.Seek(0, SeekOrigin.Begin);
                bool detectSourceMapLineNumber = true;

                while (!reader.EndOfStream)
                {
                    string lineText;
                    if (detectSourceMapLineNumber)
                    {
                        if (reader.BaseStream.Position >= sourceMappingUrlPosition)
                        {
                            // basestream position uses buffering, so it returns the length of buffered text and not the actual
                            // location we are currently at which is.. disappointing..
                            // the following extension method calculates the actual position.
                            var actualPosition = reader.GetActualPosition();
                            lineText = reader.ReadLine();
                            lineCount = lineCount + 1;

                            // Check if we are now at the position of the source map url line,
                            if (actualPosition == sourceMappingUrlPosition)
                            {
                                // we want to omit it from being written to the bundle - 
                                // write an empty line instead.
                                lineNumberOfSourceMap = lineCount;
                                writer.WriteLine();
                                detectSourceMapLineNumber = false;
                                continue;
                            }

                            writer.WriteLine(lineText);
                            continue;
                        }
                    }

                    lineText = reader.ReadLine();
                    lineCount = lineCount + 1;
                    writer.WriteLine(lineText);
                }

                sourceMappingUrlLineNumber = lineNumberOfSourceMap;
                linesWritten = lineCount;
            }
        }
    }
}