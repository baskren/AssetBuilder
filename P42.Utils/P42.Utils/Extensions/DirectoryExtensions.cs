using System;
using System.IO;

namespace P42.Utils
{
    public static class DirectoryExtensions
    {
        public static DirectoryInfo AssureExists(string fullPath)
        {
            if (!Directory.Exists(fullPath))
                return Directory.CreateDirectory(fullPath);
            return new DirectoryInfo(fullPath);
        }
    }
}
