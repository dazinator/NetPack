using System;
using System.Text;

namespace NetPack.File
{
    public class SubPathInfo
    {
        public string FileName { get; set; }
        public string Directory { get; set; }

        public static SubPathInfo Parse(string subpath)
        {

            if (string.IsNullOrWhiteSpace(subpath))
            {
                throw new ArgumentException("subpath");
            }

            var subPath = new SubPathInfo();


            var builder = new StringBuilder(subpath.Length);

            var indexOfLastSeperator = subpath.LastIndexOf('/');
            if (indexOfLastSeperator != -1)
            {
                // has directory portion.
                for (int i = 0; i <= indexOfLastSeperator; i++)
                {
                    var currentChar = subpath[i];
                    if (currentChar == '/')
                    {
                        if (i == 0 || i == indexOfLastSeperator) // omit a starting and trailing slash (/)
                        {
                            continue;
                        }
                    }
                    builder.Append(currentChar);
                }
            }

            subPath.Directory = builder.ToString();
            builder.Clear();

            // now append filename portion
            if (subpath.Length > indexOfLastSeperator + 1)
            {
                for (int c = indexOfLastSeperator + 1; c < subpath.Length; c++)
                {
                    var currentChar = subpath[c];
                    builder.Append(currentChar);
                }
            }

            subPath.FileName = builder.ToString();
            return subPath;

        }

        public override string ToString()
        {
            return $"{Directory}/{FileName}";
        }
    }
}