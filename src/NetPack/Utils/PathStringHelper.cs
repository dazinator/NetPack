using System;

namespace Microsoft.AspNetCore.Http
{
    public static class PathStringHelper
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


        public static (PathString Directory, string FileName) SplitPath(PathString path)
        {
            string pathStr = path.Value;

            // Ensure the path starts with a '/'
            if (!pathStr.StartsWith("/"))
            {
                pathStr = "/" + pathStr;
            }

            // Trim any trailing slashes
            bool endsWithSlash = pathStr.EndsWith("/");
            pathStr = pathStr.TrimEnd('/');

            // Split the path into segments
            var segments = pathStr.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
            {
                return (PathString.Empty, string.Empty);
            }

            // Identify the last segment
            string lastSegment = segments[segments.Length - 1];

            // Determine if the last segment is a directory or file
            bool isFile = !endsWithSlash && IsFileName(lastSegment);

            // Construct directory and file name
            string directory = isFile ? string.Join('/', segments[..^1]) : string.Join('/', segments);
            string fileName = isFile ? lastSegment : string.Empty;

            return (new PathString("/" + directory), fileName);
        }

        private static bool IsFileName(string segment)
        {
            if (segment.StartsWith(".") && segment.LastIndexOf(".") == 0)
            {
                return false; // Starts with a dot and no other dots, it's a directory
            }
            if (segment.Contains("*") || segment.Contains("?"))
            {
                return false; // Contains glob pattern characters, it's a directory
            }
            if (segment.Contains("."))
            {
                return true; // Contains a dot, it's a file name
            }
            return true; // Default to file name if no other conditions match
        }
        
    }
}
