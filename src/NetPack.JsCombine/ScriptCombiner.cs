using System;
using System.IO;
using System.Threading.Tasks;
using NetPack.Extensions;
using NetPack.Utils;

namespace NetPack.JsCombine
{
    public class ScriptCombiner
    {
        public async Task<CombinedScriptInfo> AddScript(Stream sourceStream, StreamWriter writer)
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
                linesWritten = await CombineWithoutSourceMap(sourceStream, writer);
                scriptInfo.LineCount = linesWritten;
                return scriptInfo;
            }

            // A source map is present, copy the script to the bundle but omit the sourcemappingurl.
            var lineInfo = await CombineWithSourceMap(sourceMappingUrlPosition, sourceStream, writer);

            scriptInfo.LineCount = lineInfo.Item1;
            scriptInfo.SourceMapDeclaration.LineNumber = lineInfo.Item2;
            return scriptInfo;
        }

        private async Task<int> CombineWithoutSourceMap(Stream sourceStream, StreamWriter writer)
        {
            int lineCount = 0;
            sourceStream.Position = 0;
            using (var reader = new StreamReader(sourceStream))
            {
                reader.BaseStream.Position = 0;
                // reader.DiscardBufferedData();
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    lineCount = lineCount + 1;
                    await writer.WriteLineAsync(line);
                }
                return lineCount;
                //  linesWritten = lineCount;
            }

        }

        private async Task<Tuple<int, int>> CombineWithSourceMap(long sourceMappingUrlPosition, Stream sourceStream, StreamWriter writer)
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
                            lineText = await reader.ReadLineAsync();
                            lineCount = lineCount + 1;

                            // Check if we are now at the position of the source map url line,
                            if (actualPosition == sourceMappingUrlPosition)
                            {
                                // we want to omit it from being written to the bundle - 
                                // write an empty line instead.
                                lineNumberOfSourceMap = lineCount;
                                await writer.WriteLineAsync();
                                detectSourceMapLineNumber = false;
                                continue;
                            }

                            await writer.WriteLineAsync(lineText);
                            continue;
                        }
                    }

                    lineText = await reader.ReadLineAsync();
                    lineCount = lineCount + 1;
                    await writer.WriteLineAsync(lineText);
                }

                // sourceMappingUrlLineNumber = lineNumberOfSourceMap;
                // linesWritten = lineCount;

                return new Tuple<int, int>(lineCount, lineNumberOfSourceMap);
            }
        }
    }
}