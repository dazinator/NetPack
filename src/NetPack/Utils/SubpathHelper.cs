using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;

namespace NetPack
{
    // ReSharper disable once CheckNamespace
    // Extension method put in root namespace for discoverability purposes.
    public static class SubpathHelper
    {
        private readonly static char[] PathSegmentSeperators = new char[]{'/'};
        //public static PathString Parse(string subPath)
        //{
        //    if (string.IsNullOrWhiteSpace(subPath))
        //    {
        //        return new PathString("/");
        //    }

        //    if (!subPath.StartsWith("/"))
        //    {
        //        return new PathString($"/{subPath}");
        //    }

        //    return new PathString($"{subPath}");

        //}


        //public static PathString MakeRelative(this PathString fromPath, PathString targetPath)
        //{
        //    //var uri = fromPath.ToUriComponent();


        //    var fromAbsolutePath = $"fakeroot:/{fromPath}";
        //    var toAbsolutePath = $"fakeroot:/{targetPath}";
        //    var fromUri = new Uri(fromAbsolutePath);
        //    var toUri = new Uri(toAbsolutePath);
        //    var relativePath = fromUri.MakeRelativeUri(toUri);
        //    var relativePathString = relativePath.GetComponents(UriComponents.HostAndPort | UriComponents.PathAndQuery, UriFormat.Unescaped);
        //    relativePathString = relativePathString.TrimEnd('/');
        //    return Parse(relativePathString);

        //}


        /// <summary>
        /// Creates a relative path from one subpath to another subpath.
        /// </summary>
        /// <param name="fromDirectory">
        /// The subpath indicating a directory that you want to create a relative subpath to navigate from.
        /// </param>
        /// <param name="toSubPath">
        /// The subpath that you want to create a relative subpath to navigate to.
        /// </param>
        /// <returns>
        /// The relative subpath.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string MakeRelativeSubpath(string fromDirectory, string toSubPath)
        {
            if (fromDirectory == null)
                throw new ArgumentNullException("fromDirectory");

            if (toSubPath == null)
                throw new ArgumentNullException("toPath");

            bool isRooted = (Path.IsPathRooted(fromDirectory) && Path.IsPathRooted(toSubPath));

            if (isRooted)
            {
                bool isDifferentRoot = (string.Compare(Path.GetPathRoot(fromDirectory), Path.GetPathRoot(toSubPath), true) != 0);

                if (isDifferentRoot)
                    return toSubPath;
            }


          //  string[] fromDirectories = fromSubPath.Split(PathSegmentSeperators, StringSplitOptions.RemoveEmptyEntries);
          //  string[] toDirectories = toSubPath.Split(PathSegmentSeperators, StringSplitOptions.RemoveEmptyEntries);

            string[] fromDirectories = fromDirectory.Split('/');
            string[] toDirectories = toSubPath.Split('/');

            var relativePath = new List<string>(fromDirectories.Length + toDirectories.Length);

            int length = Math.Min(fromDirectories.Length, toDirectories.Length);

            int lastCommonRoot = -1;

            // find common root
            for (int x = 0; x < length; x++)
            {
                if (string.Compare(fromDirectories[x], toDirectories[x], true) != 0)
                    break;

                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
                return toSubPath;

            // add relative folders in from path
            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
            {
                if (fromDirectories[x].Length > 0)
                    relativePath.Add("..");
            }

            // add to folders to path
            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
            {
                relativePath.Add(toDirectories[x]);
            }

            // create relative path
            string[] relativeParts = new string[relativePath.Count];
            relativePath.CopyTo(relativeParts, 0);

            string newPath = string.Join("/", relativeParts);
            return newPath;
        }

        ///// <summary>
        ///// Creates a relative path from one file or folder to another.
        ///// </summary>
        ///// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        ///// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        ///// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        ///// <exception cref="ArgumentNullException"></exception>
        ///// <exception cref="UriFormatException"></exception>
        ///// <exception cref="InvalidOperationException"></exception>
        //public static String MakeRelativePath(String fromPath, String toPath)
        //{
        //    if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
        //    if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

        //    var uriBuilder = new UriBuilder(fromPath.Replace('/', Path.DirectorySeparatorChar));
        //    uriBuilder.Scheme = "x:\\";

        //    var uriToBuilder = new UriBuilder(toPath.Replace('/', Path.PathSeparator));
        //    uriBuilder.Scheme = "x:\\";

        //    Uri fromUri = uriBuilder.Uri;
        //    Uri toUri = uriToBuilder.Uri;

        //    Uri relativeUri = fromUri.MakeRelativeUri(toUri);
        //    String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        //    if (toUri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
        //    {
        //        relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        //    }

        //    return relativePath;
        //}


    }
}