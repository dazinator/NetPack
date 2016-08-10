using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using NetPack.File;
using NetPack.Pipes;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace NetPack.Tests
{
    public class ScriptCombinerTests
    {


        public const string JsContentTwo = "define(\"IContentTwo\", [\"require\", \"exports\"], function (require, exports) { \"use strict\"; });";

        private ITestOutputHelper _output;
        public ScriptCombinerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async void Can_Combine_Source_Files_With_SourceMapUrls()
        {
            // set up a couple of in memory files, for fileA and fileB
            // that both have source mapping url comments.
            var fileProvider = new InMemoryFileProvider();
            var pathFileA = SubPathInfo.Parse("wwwroot/fileA.js");
            var contentFileA = GenerateString(8000) + Environment.NewLine + "//# sourceMappingURL=/" + pathFileA.ToString();
            fileProvider.AddFile(pathFileA, contentFileA);

            var pathFileB = SubPathInfo.Parse("wwwroot/fileB.js");
            var contentFileB = JsContentTwo + Environment.NewLine + "//# sourceMappingURL=/" + pathFileB.ToString();
            fileProvider.AddFile(pathFileB, contentFileB);

            var fileInfoA = fileProvider.GetFileInfo(pathFileA);
            var fileInfoB = fileProvider.GetFileInfo(pathFileB);

            var files = new List<IFileInfo>();
            files.Add(fileInfoA);
            files.Add(fileInfoB);
            var sut = new ScriptCombiner();

            var ms = new MemoryStream();
            var scriptInfoList = new List<ScriptInfo>();

            int newLineBytes = System.Environment.NewLine.Length;
            long bytesRead = 0;

            foreach (var file in files)
            {

                Dictionary<int, long> linePositions = new Dictionary<int, long>();
                int lineCount = 0;
                using (var testReader = new StreamReader(file.CreateReadStream()))
                {
                    while (!testReader.EndOfStream)
                    {
                        lineCount = lineCount + 1;
                        long pos = GetActualPosition(testReader);
                        var lineText = testReader.ReadLine();
                        linePositions.Add(lineCount, pos);
                    }
                }

                using (var fileStream = file.CreateReadStream())
                {

                    using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                    {
                        var scriptInfo = sut.AddScript(fileStream, writer);
                        Assert.Equal(scriptInfo.LineCount, lineCount);
                        scriptInfoList.Add(scriptInfo);
                    }


                }
            }

            // after combining the two files, we should have an output to memory stream containing both files
            // but wuthout sourcemapping urls.
            ms.Position = 0;
            var outputStreamReader = new StreamReader(ms);
            var text = outputStreamReader.ReadToEnd();
            _output.WriteLine(text);
            Assert.DoesNotContain("//# sourceMappingURL=", text);

        }

        private string GenerateString(int length)
        {
            var builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                builder.Append("A");
            }

            return builder.ToString();
        }


        public static long GetActualPosition(StreamReader reader)
        {
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField;

            // The current buffer of decoded characters
            char[] charBuffer = (char[])reader.GetType().InvokeMember("charBuffer", flags, null, reader, null);

            // The index of the next char to be read from charBuffer
            int charPos = (int)reader.GetType().InvokeMember("charPos", flags, null, reader, null);

            // The number of decoded chars presently used in charBuffer
            int charLen = (int)reader.GetType().InvokeMember("charLen", flags, null, reader, null);

            // The current buffer of read bytes (byteBuffer.Length = 1024; this is critical).
            byte[] byteBuffer = (byte[])reader.GetType().InvokeMember("byteBuffer", flags, null, reader, null);

            // The number of bytes read while advancing reader.BaseStream.Position to (re)fill charBuffer
            int byteLen = (int)reader.GetType().InvokeMember("byteLen", flags, null, reader, null);

            // The number of bytes the remaining chars use in the original encoding.
            int numBytesLeft = reader.CurrentEncoding.GetByteCount(charBuffer, charPos, charLen - charPos);

            // For variable-byte encodings, deal with partial chars at the end of the buffer
            int numFragments = 0;
            if (byteLen > 0 && !reader.CurrentEncoding.IsSingleByte)
            {
                if (reader.CurrentEncoding.CodePage == 65001) // UTF-8
                {
                    byte byteCountMask = 0;
                    while ((byteBuffer[byteLen - numFragments - 1] >> 6) == 2) // if the byte is "10xx xxxx", it's a continuation-byte
                        byteCountMask |= (byte)(1 << ++numFragments); // count bytes & build the "complete char" mask
                    if ((byteBuffer[byteLen - numFragments - 1] >> 6) == 3) // if the byte is "11xx xxxx", it starts a multi-byte char.
                        byteCountMask |= (byte)(1 << ++numFragments); // count bytes & build the "complete char" mask
                                                                      // see if we found as many bytes as the leading-byte says to expect
                    if (numFragments > 1 && ((byteBuffer[byteLen - numFragments] >> 7 - numFragments) == byteCountMask))
                        numFragments = 0; // no partial-char in the byte-buffer to account for
                }
                else if (reader.CurrentEncoding.CodePage == 1200) // UTF-16LE
                {
                    if (byteBuffer[byteLen - 1] >= 0xd8) // high-surrogate
                        numFragments = 2; // account for the partial character
                }
                else if (reader.CurrentEncoding.CodePage == 1201) // UTF-16BE
                {
                    if (byteBuffer[byteLen - 2] >= 0xd8) // high-surrogate
                        numFragments = 2; // account for the partial character
                }
            }
            return reader.BaseStream.Position - numBytesLeft - numFragments;
        }

    }


}