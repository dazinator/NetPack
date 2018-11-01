using System;

namespace Microsoft.AspNetCore.Http
{
    public static class PathStringUtils
    {
        private static readonly char[] _pathSplitChars = new char[] { '/', '\\' };

        public static PathString ToPathString(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new PathString();
            }

            if (path.StartsWith("/"))
            {
                return (PathString)path;
            }

            return new PathString($"/{path}");
        }


        public static void GetPathAndFilename(string fullPath, out PathString rootPath, out string fileName)
        {
            string[] split = fullPath.Split(_pathSplitChars, StringSplitOptions.RemoveEmptyEntries);
            rootPath = "/";

            if (split.Length > 1)
            {
                for (int i = 0; i < (split.Length - 1); i++)
                {
                    rootPath = rootPath + split[i] + "/";
                }
            }

            fileName = split[split.Length - 1];
        }
    }
}
