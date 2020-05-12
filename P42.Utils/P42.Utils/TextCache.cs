using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P42.Utils
{
    public static class TextCache
    {
        const string DownloadStorageFolderName = "P42.Utils.TextCache";

        static string FolderPath(string folderName)
        {
            DirectoryExtensions.AssureExists(Environment.ApplicationCachePath);
            var root = Path.Combine(Environment.ApplicationCachePath, DownloadStorageFolderName);
            DirectoryExtensions.AssureExists(root);

            if (string.IsNullOrWhiteSpace(folderName))
                return root;

            var folderPath = Path.Combine(root, folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }

        static TextCache()
        {
            // can be used for caching text between session.  
        }


        //static readonly object _locker = new object();
        //static readonly Dictionary<string, Task<bool>> _downloadTasks = new Dictionary<string, Task<bool>>();
        //static readonly System.Security.Cryptography.MD5 _md5 = System.Security.Cryptography.MD5.Create();

        public static List<string> List(string folderName)
        {
            var folderPath = FolderPath(folderName);
            var files = System.IO.Directory.EnumerateFiles(folderPath);
            return files.ToList();
        }

        public static void Store(string text, string key, string folderName = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            var path = CachedPath(key, folderName);

            if (string.IsNullOrWhiteSpace(path))
                return;

            var guid = Guid.NewGuid().ToString();
            var tmpPath = CachedPath(guid, folderName);

            System.IO.File.WriteAllText(tmpPath, text);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            System.IO.File.Move(tmpPath, path);

            System.IO.File.Delete(tmpPath);
        }

        public static string Recall(string key, string folderName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return null;

                var path = CachedPath(key, folderName);

                if (string.IsNullOrWhiteSpace(path))
                    return null;
                if (System.IO.File.Exists(path))
                    return System.IO.File.ReadAllText(path);
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }

        public static StreamReader GetStreamReader(string key, string folderName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return null;

                var path = CachedPath(key, folderName);

                if (string.IsNullOrWhiteSpace(path))
                    return null;
                if (System.IO.File.Exists(path))
                    return new StreamReader(path);
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }

        static string CachedPath(string key, string folderName = null)
        {
            var fileName = key.Trim().ToMd5HashString();
            return Path.Combine(FolderPath(folderName), fileName);
        }

        public static bool IsCached(string key, string folderName = null)
        {
            var path = CachedPath(key, folderName);
            return path != null && System.IO.File.Exists(path);
        }

        public static bool Clear(string key = null, string folderName = null)
            => Clear(DateTime.MinValue, key, folderName);

        public static bool Clear(TimeSpan timeSpan, string key = null, string folderName = null)
            => Clear(DateTime.Now - timeSpan, key, folderName);

        public static bool Clear(DateTime dateTime, string key = null, string folderName = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                // complete clear
                var folderPath = FolderPath(folderName);
                if (System.IO.Directory.Exists(folderPath))
                {
                    var files = System.IO.Directory.EnumerateFiles(folderPath);
                    bool filesRemaining = false;
                    foreach (var file in files)
                    {
                        if (System.IO.File.Exists(file))
                        {
                            if (System.IO.File.GetLastWriteTime(file) < dateTime)
                                System.IO.File.Delete(file);
                            else
                                filesRemaining = true;
                        }
                    }
                    if (!filesRemaining && folderPath != FolderPath(null))
                        System.IO.Directory.Delete(folderPath);
                    return true;
                }
                return false;
            }
            var path = CachedPath(key, folderName);
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                if (System.IO.File.GetLastWriteTime(path) < dateTime)
                {
                    System.IO.File.Delete(path);
                    return true;
                }
            }
            return false;
        }

    }
}
