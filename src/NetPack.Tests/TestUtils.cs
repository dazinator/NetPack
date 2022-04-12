using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Moq;

namespace NetPack.Tests
{
    public static class TestUtils
    {
        public static string GenerateString(int length)
        {
            var builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                builder.Append("A");
            }

            return builder.ToString();
        }

    }



}