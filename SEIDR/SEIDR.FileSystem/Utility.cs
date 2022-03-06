using System;
using System.IO;

namespace SEIDR.FileSystem
{
    public static class Utility
    {
        public static bool Contains(this string value, string test, StringComparison comparison)
        {
            return value.IndexOf(test, comparison) >= 0;
        }
        /// <summary>
        /// Removes tail slash from path.
        /// </summary>
        /// <param name="path">path to remove tail slash</param>
        /// <returns>path without tail slash</returns>
        public static string RemoveTailSlash(string path)
        {
            string result = path;
            if (!string.IsNullOrEmpty(result))
            {
                if (result.EndsWith("\\") || result.EndsWith("//"))
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns list of files from given path.
        /// </summary>
        /// <param name="path">path to get files</param>
        /// <param name="defaultFilter">Default filter for files if mask not provided at path, if is empty will be used *.*</param>
        /// <returns>list of files from given path</returns>
        public static string[] GetFiles(string path, string defaultFilter)
        {
            string[] result = new string[0];
            if (!string.IsNullOrEmpty(path))
            {
                defaultFilter = string.IsNullOrEmpty(defaultFilter) ? "*.*" : defaultFilter;
                string filter = Path.HasExtension(path) ? Path.GetFileName(path) : defaultFilter;
                string sourcePath = Path.HasExtension(path) ? Path.GetDirectoryName(path) : Path.GetFullPath(path);
                sourcePath = Utility.RemoveTailSlash(sourcePath);
                filter = String.IsNullOrEmpty(filter) ? "*.*" : filter;

                result = Directory.GetFiles(sourcePath, filter, SearchOption.TopDirectoryOnly);
            }

            return result;
        }

        /// <summary>
        /// Check path existence.
        /// </summary>
        /// <param name="path">path to check, can be with file name or mask</param>
        /// <returns>Returns true if path exists, otherwise false</returns>
        public static bool IsPathExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            string tmpPath = Path.HasExtension(path) ? Path.GetDirectoryName(path) : path;
            return Directory.Exists(tmpPath);
        }
    }
}
