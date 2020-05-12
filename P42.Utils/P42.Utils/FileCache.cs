using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P42.Utils
{
    public static class FileCache
    {
        const string FileCacheFolderName = "P42.Utils.FileCache";

        static string FolderPath(string folderName = null)
        {
            DirectoryExtensions.AssureExists(Environment.ApplicationDataPath);
            var root = Path.Combine(Environment.ApplicationDataPath, FileCacheFolderName);
            DirectoryExtensions.AssureExists(root);

            if (string.IsNullOrWhiteSpace(folderName))
                return root;

            var folderPath = Path.Combine(root, folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }

        static readonly object _locker = new object();
        static readonly Dictionary<string, Task<bool>> _downloadTasks = new Dictionary<string, Task<bool>>();

        public static List<string> List(string folderName)
        {
            var folderPath = FolderPath(folderName);
            var files = System.IO.Directory.EnumerateFiles(folderPath);
            return files.ToList();
        }


        public static string Cache(string sourceFilePath, string folderName = null)
        {
            var task = Task.Run(() => CacheAsync(sourceFilePath, folderName));
            return task.Result;
        }

        static string CachedPath(string sourceFilePath, string folderName = null)
        {
            var fileName = sourceFilePath.Trim().ToMd5HashString();
            return Path.Combine(FolderPath(folderName), fileName);
        }

        public static async Task<string> CacheAsync(string sourceFilePath, string folderName = null)
        {
            //var hash = _md5.ComputeHash(Encoding.UTF8.GetBytes(sourceFilePath.Trim()));
            //var fileName = string.Join("", hash.Select(x => x.ToString("x2")));
            var fileName = CachedPath(sourceFilePath, folderName);
            return await GetCacheAsync(sourceFilePath, fileName, folderName);
        }

        static async Task<string> GetCacheAsync(string sourceFilePath, string destFileName, string folderName = null)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || string.IsNullOrWhiteSpace(destFileName))
                return null;
            try
            {
                var destPath = Path.Combine(FolderPath(folderName), destFileName);
                if (System.IO.File.Exists(destPath) && !_downloadTasks.ContainsKey(destPath))
                {
                    System.Diagnostics.Debug.WriteLine("DownloadCache: [" + destPath + "] exists as [" + destFileName + "]");
                    return destPath;
                }

                var success = await CacheFileAsync(sourceFilePath, destPath);
                return success ? destPath : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }

        static Task<bool> CacheFileAsync(string sourcePath, string destPath)
        {
            lock (_locker)
            {
                //Task<bool> task = null;
                //if (_downloadTasks.TryGetValue(fileName, out task))
                if (_downloadTasks.TryGetValue(sourcePath, out Task<bool> task))
                    return task;

                _downloadTasks.Add(sourcePath, task = CopyTask(sourcePath, destPath));
                return task;
            }
        }

        static async Task<bool> CopyTask(string sourcePath, string destPath)
        {
            try
            {
                using (var outFileStream = System.IO.File.Create(destPath))
                using (var sourceStream = System.IO.File.OpenRead(sourcePath))
                {
                    await sourceStream.CopyToAsync(outFileStream, 81920);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            if (System.IO.File.Exists(destPath))
                System.IO.File.Delete(destPath);
            return false;
        }


        public static bool Clear(string sourceFilePath = null, string folderName = null)
            => Clear(DateTime.MinValue, sourceFilePath, folderName);

        public static bool Clear(TimeSpan timeSpan, string sourceFilePath = null, string folderName = null)
            => Clear(DateTime.Now - timeSpan, sourceFilePath, folderName);

        public static bool Clear(DateTime dateTime, string sourceFilePath = null, string folderName = null)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
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
            var path = CachedPath(sourceFilePath, folderName);
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
