using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Linq;

namespace P42.Utils
{
    static public class EmbeddedResourceCache
    {
        const string LocalStorageFolderName = "P42.Utils.EmbeddedResourceCache";

        // DO NOT CHANGE Environment.ApplicationDataPath to another path.  This is used to pass EmbeddedResource Fonts to UWP Text elements and there is zero flexibility here.
        public static string FolderPath(Assembly assembly, string folderName = null)
        {
            DirectoryExtensions.AssureExists(Environment.ApplicationDataPath);
            var root = Path.Combine(Environment.ApplicationDataPath, LocalStorageFolderName);
            DirectoryExtensions.AssureExists(root);
            if (assembly != null)
            {
                root = Path.Combine(root, assembly.GetName().Name);
                DirectoryExtensions.AssureExists(root);
            }

            if (string.IsNullOrWhiteSpace(folderName))
                return root;

            var folderPath = Path.Combine(root, folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }

        static EmbeddedResourceCache()
        {
            var path = FolderPath(null);
            Directory.Delete(path, true);
        }

        static readonly object _locker = new object();
        static readonly Dictionary<string, Task<bool>> _cacheTasks = new Dictionary<string, Task<bool>>();

        public static List<string> List(Assembly assembly, string folderName)
        {
            var folderPath = FolderPath(assembly, folderName);
            var files = Directory.EnumerateFiles(folderPath);
            return files.ToList();
        }



        public static Stream GetStream(string resourceId, Assembly assembly, string folderName = null)
        {
            var task = Task.Run(() => GetStreamAsync(resourceId, assembly, folderName));
            return task.Result;
        }

        public static async Task<Stream> GetStreamAsync(string resourceId, Assembly assembly = null, string folderName = null)
        {
            assembly = assembly ?? Environment.EmbeddedResourceAssemblyResolver?.Invoke(resourceId);
            if (assembly == null)
                return null;
            var fileName = await LocalStorageSubPathForEmbeddedResourceAsync(resourceId, assembly, folderName);
            if (fileName == null)
                return null;
            var result = File.Open(Path.Combine(FolderPath(assembly, folderName), fileName), FileMode.Open);
            return result;
        }


        public static string ApplicationUri(string resourceId, Assembly assembly = null, string folderName = null)
        {
            var localStorageFileName = LocalStorageSubPathForEmbeddedResource(resourceId, assembly, folderName);
            var asmName = assembly?.GetName().Name;
            var updatedLocalStorageFileName = localStorageFileName.Replace('\\', '/');
            var uriString = "ms-appdata:///local/" + LocalStorageFolderName + "/" + (asmName is null ? "" : asmName + "/") + updatedLocalStorageFileName;
            return uriString;
        }

        public static string LocalStorageSubPathForEmbeddedResource(string resourceId, Assembly assembly = null, string folderName = null)
        {
            var task = Task.Run(() => LocalStorageSubPathForEmbeddedResourceAsync(resourceId, assembly, folderName));
            return task.Result;
        }

        public static string LocalStorageFullPathForEmbeddedResource(string resourceId, Assembly assembly = null, string folderName = null)
        {
            var task = Task.Run(async () =>
            {
                var path = await LocalStorageFullPathForEmbeddedResourceAsync(resourceId, assembly, folderName);
                return path;
            });

            return task.Result;
        }

        public static async Task<string> LocalStorageFullPathForEmbeddedResourceAsync(string resourceId, Assembly assembly = null, string folderName = null)
        {
            if (await LocalStorageSubPathForEmbeddedResourceAsync(resourceId, assembly, folderName) is string subPath)
            {
                var path = Path.Combine(FolderPath(assembly, folderName), subPath);
                return path;
            }
            return null;
        }

        public static bool Clear(string resourceId = null, Assembly assembly = null, string folderName = null)
            => Clear(DateTime.MinValue, resourceId, assembly, folderName);

        public static bool Clear(TimeSpan timeSpan, string resourceId = null, Assembly assembly = null, string folderName = null)
            => Clear(DateTime.Now - timeSpan, resourceId, assembly, folderName);

        public static bool Clear(DateTime dateTime, string resourceId = null, Assembly assembly = null, string folderName = null)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                // complete clear
                var folderPath = FolderPath(assembly, folderName);
                if (Directory.Exists(folderPath))
                {
                    var files = Directory.EnumerateFiles(folderPath);
                    bool filesRemaining = false;
                    foreach (var file in files)
                    {
                        if (File.Exists(file))
                        {
                            if (File.GetLastWriteTime(file) < dateTime)
                                File.Delete(file);
                            else
                                filesRemaining = true;
                        }
                    }
                    if (!filesRemaining && folderPath != FolderPath(null))
                        Directory.Delete(folderPath);
                    return true;
                }
                return false;
            }

            assembly = assembly ?? Environment.EmbeddedResourceAssemblyResolver?.Invoke(resourceId);
            if (assembly == null)
                return false;

            var path = Path.Combine(FolderPath(assembly, folderName), resourceId);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                if (File.GetLastWriteTime(path) < dateTime)
                {
                    File.Delete(path);
                    return true;
                }
            }
            return false;
        }


        public static async Task<string> LocalStorageSubPathForEmbeddedResourceAsync(string resourceId, Assembly assembly = null, string folderName = null)
        {
            assembly = assembly ?? Environment.EmbeddedResourceAssemblyResolver?.Invoke(resourceId);
            if (assembly == null)
                return null;

            var fileName = resourceId;
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var isZip = fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

            if (fileName.StartsWith(folderName + ".", StringComparison.Ordinal))
                fileName = fileName.Substring((folderName + ".").Length);

            try
            {
                var path = isZip
                    ? Path.Combine(FolderPath(assembly), resourceId)
                    : Path.Combine(FolderPath(assembly, folderName), fileName);
                System.Diagnostics.Debug.WriteLine("PATH=[" + path + "]");

                //var exists = (isZip && Directory.Exists(path)) || (!isZip && File.Exists(path));

                if (!_cacheTasks.ContainsKey(path) && ( File.Exists(path) || Directory.Exists(path)) )
                {
                    System.Diagnostics.Debug.WriteLine("EmbeddedResourceCache: [" + assembly.GetName().Name + ";" + resourceId + "] exists as [" + path + "]");
                    if (isZip)
                        return FolderPath(assembly, folderName);
                    return fileName;
                }

                if (CacheEmbeddedResource(resourceId, assembly, path) is Task<bool> task)
                {
                    if (await task)
                    {
                        if (isZip)
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(path, FolderPath(assembly, folderName));
                            _cacheTasks.Remove(path);
                            return FolderPath(assembly, folderName);
                        }
                        _cacheTasks.Remove(path);
                        return fileName;
                    }
                    _cacheTasks.Remove(path);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }

        static Task<bool> CacheEmbeddedResource(string resourceId, Assembly assembly, string fileName)
        {
            lock (_locker)
            {
                if (_cacheTasks.TryGetValue(fileName, out Task<bool> task))
                    return task;
                _cacheTasks.Add(fileName, task = CacheTask(resourceId, assembly, fileName));
                return task;
            }
        }

#pragma warning disable CS1998
        static async Task<bool> CacheTask(string resourceId, Assembly assembly, string path)
#pragma warning restore CS1998
        {
            try
            {
                using (var stream = EmbeddedResource.GetStream(resourceId, assembly))
                {
                    if (stream is null)
                    {
                        Console.WriteLine("Cannot find EmbeddedResource [" + resourceId + "] in assembly [" + assembly.FullName + "].   Here are the ResourceIds in that assembly:");
                        foreach (var id in assembly.GetManifestResourceNames())
                            Console.WriteLine("\t" + id);
                        Console.WriteLine("");
                    }
                    else
                    {
                        if (File.Exists(path))
                            System.Diagnostics.Debug.WriteLine("DownloadTask: FILE ALREADY EXISTS [" + path + "] [" + assembly.GetName().Name + ";" + resourceId + "]");

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            stream.CopyTo(fileStream);
                            fileStream.Flush(true);
                            var length = fileStream.Length;
                            System.Diagnostics.Debug.WriteLine("DownloadTask: file written [" + path + "] [" + assembly.GetName().Name + ";" + resourceId + "] length=[" + length + "] name=[" + fileStream.Name + "] pos=[" + fileStream.Position + "]");
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            if (File.Exists(path))
                File.Delete(path);
            return false;
        }
    }
}
