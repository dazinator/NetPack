using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NetPack.File
{
    public class SubPathInfo
    {

        protected SubPathInfo(string directory, string filename)
        {
            Directory = directory;
            FileName = filename;
            CheckPattern();
        }

        public string FileName { get; }
        public string Directory { get; }

        public bool IsPattern { get; set; }

        public bool IsFile
        {
            get { return !IsPattern && !string.IsNullOrWhiteSpace(FileName); }
        }

        public void CheckPattern()
        {
            bool patternValidationEnabled = true;
            bool isWithinRangeBlock = false;
            var allChars = this.ToString();

            for (int i = 0; i <= allChars.Length -1; i++)
            {
                var currentChar = allChars[i];
                if (patternValidationEnabled)
                {
                    if (IsStarOrQuestionMarkChar(currentChar))
                    {
                        IsPattern = true;
                    }
                    else if (IsStartRangeChar(currentChar))
                    {
                        if (isWithinRangeBlock)
                        {
                            throw new ArgumentException(
                                "Unsupported globbing pattern. A pattern cannot use nested [ characters. Expected a cosing bracket.");
                        }

                        isWithinRangeBlock = true;
                        IsPattern = true;
                    }
                    else if (IsEndRangeChar(currentChar))
                    {
                        // maybe at end of range block.
                        if (!isWithinRangeBlock)
                        {
                            throw new ArgumentException(
                              "Unsupported globbing pattern. A pattern cannot use a ] character without an opening [ character. Expected an opening bracket.");
                        }
                        isWithinRangeBlock = false;
                    }

                }
            }
        }

        public static SubPathInfo Parse(string subpath)
        {
            if (string.IsNullOrWhiteSpace(subpath))
            {
                throw new ArgumentException("subpath");
            }

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

            var directory = builder.ToString();
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

            var fileName = builder.ToString();
            var subPath = new SubPathInfo(directory, fileName);
            return subPath;
        }

        public static bool IsStarOrQuestionMarkChar(char currentChar)
        {
            if (currentChar == '*' || currentChar == '?')
            {
                return true;
            }

            return false;
        }

        public static bool IsStartRangeChar(char currentChar)
        {
            if (currentChar == '[')
            {
                return true;
            }

            return false;
        }

        public static bool IsEndRangeChar(char currentChar)
        {
            if (currentChar == ']')
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"{Directory}/{FileName}";
        }

        public bool IsMatch(SubPathInfo subPath)
        {
            if (subPath.IsPattern)
            {
                return this.Like(subPath.ToString());
            }
            if (string.IsNullOrEmpty(subPath.FileName) && IsInDirectory(subPath))
            {
                return true;
            }
            return this.Equals(subPath);
        }

        public bool IsInDirectory(SubPathInfo directory)
        {
            var match = directory.Directory == Directory;
            return match;
        }

        /// <summary>
        /// Compares the subpath against a given pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
        /// <returns><c>true</c> if the subpath matches the given pattern; otherwise <c>false</c>.</returns>
        public bool Like(string pattern)
        {

            var regex = new Regex("^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var path = this.ToString();

            var isMatch = regex.IsMatch(path);
            return isMatch;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Point return false.
            SubPathInfo p = obj as SubPathInfo;
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Directory == p.Directory) && (FileName == p.FileName);
        }

        public override int GetHashCode()
        {
            int hashCode = Directory.GetHashCode() + FileName.GetHashCode();
            return hashCode;
        }

    }

}