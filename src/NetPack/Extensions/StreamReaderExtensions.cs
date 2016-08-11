using System;
using System.IO;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace NetPack.Extensions
{
    public static class StreamReaderExtensions
    {

        private static System.Reflection.BindingFlags defaultFlags = System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField;
        private static Type streamReaderType = typeof(StreamReader);

        private static FieldInfo charBufferField = streamReaderType.GetField("charBuffer", defaultFlags);
        private static FieldInfo charPosField = streamReaderType.GetField("charPos", defaultFlags);
        private static FieldInfo charLenField = streamReaderType.GetField("charLen", defaultFlags);
        private static FieldInfo byteBufferField = streamReaderType.GetField("byteBuffer", defaultFlags);
        private static FieldInfo byteLenField = streamReaderType.GetField("byteLen", defaultFlags);
        
        public static long GetActualPosition(this StreamReader reader)
        {



            // var charBufferField = _type.GetField("charBuffer", _flags);
            // var charPosField = _type.GetField("charPos", _flags);
            // var charLenField = _type.GetField("charLen", _flags);
            //  var byteBufferField = _type.GetField("byteBuffer", _flags);
            //   var byteLenField = _type.GetField("byteLen", _flags);

            char[] charBuffer = (char[])charBufferField.GetValue(reader);
            int charPos = (int)charPosField.GetValue(reader);
            int charLen = (int)charLenField.GetValue(reader);
            byte[] byteBuffer = (byte[])byteBufferField.GetValue(reader);
            int byteLen = (int)byteLenField.GetValue(reader);

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