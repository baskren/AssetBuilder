using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace P42.Utils
{
    public static class BreadCrumbs
    {
        const string BreadCrumbFolderName = "P42.Utils.BreadCrumbs";

        static string _folderPath;
        static string FolderPath
        {
            get
            {
                if (_folderPath == null)
                {
                    if (!Directory.Exists(P42.Utils.Environment.ApplicationCachePath))
                        Directory.CreateDirectory(P42.Utils.Environment.ApplicationCachePath);
                    var folderPath = Path.Combine(P42.Utils.Environment.ApplicationCachePath, BreadCrumbFolderName);
                    if (!Directory.Exists(folderPath))
                    {
                        var directoryInfo = Directory.CreateDirectory(folderPath);
                        if (!directoryInfo.Exists)
                            throw new Exception("huh?  Could not create directory [" + directoryInfo + "]");
                    }
                    _folderPath = folderPath;
                }
                return _folderPath;
            }
        }

        public static bool IsEnabled
        {
            get => _streamWriter != null;
            set
            {
                if (IsEnabled != value)
                {
                    if (_streamWriter != null)
                    {
                        _streamWriter.Flush();
                        _streamWriter.Close();
                        _streamWriter.Dispose();
                        _streamWriter = null;
                    }
                    else
                    {
                        var fileName = DateTime.Now.ToString("yyyyMMdd'T'HHmmss") + ".txt";
                        var filePath = Path.Combine(FolderPath, fileName);
                        _streamWriter = File.CreateText(filePath);
                    }
                }
            }
        }

        public static Action<string, string, string, int, string> EventAction;
        public static Action<Exception, string, string, string, int, string> ExceptionAction;

        static StreamWriter _streamWriter;


        public static void Add(Type type, object crumb = null, [CallerMemberName] string method = null, [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
            => Add(type?.ToString(), crumb?.ToString(), method, lineNumber, path);

        public static void Add(string className, string crumb = null, [CallerMemberName] string method = null, [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
        {
            if (IsEnabled)
            {
                _streamWriter.WriteLine("[" + DateTime.Now.ToString("o") + "] [" + className + "." + method + ":" + lineNumber + "] [" + crumb + "] ");
                _streamWriter.Flush();
            }
            EventAction?.Invoke(className, crumb, method, lineNumber, path);
        }

        public static void AddException(Exception e, Type type, object crumb = null, [CallerMemberName] string method = null, [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
            => AddException(e, type?.ToString(), crumb?.ToString(), method, lineNumber, path);

        public static void AddException(Exception e, string className, string crumb = null, [CallerMemberName] string method = null, [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [CallerFilePath] string path = null)
        {
            if (IsEnabled)
            {
                _streamWriter.WriteLine("[" + DateTime.Now.ToString("o") + "] [" + className + "." + method + ":" + lineNumber + "] [" + crumb + "] ");
                _streamWriter.Flush();
            }
            ExceptionAction?.Invoke(e, className, crumb, method, lineNumber, path);
        }

        public static string[] List()
        {
            try
            {
                var files = Directory.GetFiles(FolderPath);
                Array.Sort(files);
                return files;
            }
            catch (Exception)
            {

            }
            return null;
        }

        public static bool Clear()
            => Clear(DateTime.MinValue);

        public static bool Clear(TimeSpan timeSpan)
            => Clear(DateTime.Now - timeSpan);

        public static bool Clear(DateTime dateTime)
        {
            // complete clear
            if (System.IO.Directory.Exists(FolderPath))
            {
                var files = System.IO.Directory.EnumerateFiles(FolderPath);
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
                return filesRemaining;
            }
            return false;
        }

    }
}
