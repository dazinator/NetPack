using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace NetPack.Extensions
{
    public static class StreamExtensions
    {
        public static void ReadExactly(this Stream input, byte[] buffer, int bytesToRead)
        {
            int index = 0;
            while (index < bytesToRead)
            {
                int read = input.Read(buffer, index, bytesToRead - index);
                if (read == 0)
                {
                    throw new EndOfStreamException
                        (String.Format("End of stream reached with {0} byte{1} left to read.",
                            bytesToRead - index,
                            bytesToRead - index == 1 ? "s" : ""));
                }
                index += read;
            }
        }
    }
}