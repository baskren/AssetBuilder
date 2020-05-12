using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace P42.Utils
{
    public static class Environment
    {
        static Environment()
        {
#if NETSTANDARD2_0_VOIDED
            //DocumentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments, System.Environment.SpecialFolderOption.Create);
            switch (GetOperatingSystem().ToLower())
            {
                /*
                case "apple":
                    var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.InternetCache, System.Environment.SpecialFolderOption.Create);
                    path = path.Remove(path.IndexOf("Caches"));
                    path = path + "ApplicationData";
                    ApplicationDataPath = path;
                    break;
                    */
                case "linux":
                    ApplicationDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData, System.Environment.SpecialFolderOption.Create);
                    break;
                default:
                    ApplicationDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData, System.Environment.SpecialFolderOption.Create);
                    break;
            }
            switch (GetOperatingSystem().ToLower())
            {
                case "linux":
                    var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal, System.Environment.SpecialFolderOption.Create);
                    path = System.IO.Path.Combine(path, ".cache");
                    ApplicationCachePath = path;
                    break;
                default:
                    ApplicationCachePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.InternetCache, System.Environment.SpecialFolderOption.Create);
                    break;
            }

            TemporaryStoragePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments, System.Environment.SpecialFolderOption.Create);
#endif
        }

        static public void Init()
        {
            MainThreadId = System.Environment.CurrentManagedThreadId;
        }

        /// <summary>
        /// Gets or sets the main thread identifier.
        /// </summary>
        /// <value>The main thread identifier.</value>
        static public int MainThreadId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:P42.Utils.Environment"/> is headless test (and thus the MainThreadId is questionable).
        /// </summary>
        /// <value><c>true</c> if is headless test; otherwise, <c>false</c>.</value>
        static public bool IsHeadlessTest { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:P42.Utils.Environment"/> is on main thread.
        /// </summary>
        /// <value><c>true</c> if is on main thread; otherwise, <c>false</c>.</value>
        public static bool IsOnMainThread => IsHeadlessTest || System.Environment.CurrentManagedThreadId == MainThreadId;

        static public Func<string, Assembly> EmbeddedResourceAssemblyResolver;

        /// <summary>
        /// Gets the operating system.
        /// </summary>
        /// <returns>The operating system.</returns>
        public static string GetOperatingSystem()
        {
            var windir = System.Environment.GetEnvironmentVariable("windir");
            if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && System.IO.Directory.Exists(windir))
                return "Windows";
            else if (System.IO.File.Exists(@"/proc/sys/kernel/ostype"))
            {
                var osType = System.IO.File.ReadAllText(@"/proc/sys/kernel/ostype");
                if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
                    // Note: Android gets here too
                    return "Linux";
                else
                {
                    throw new UnsupportedPlatformException(osType);
                }
            }
            else if (System.IO.File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
            {
                // Note: iOS gets here too
                return "Apple";
            }
            else
            {
                throw new UnsupportedPlatformException();
            }
        }


        public static Action PlatformPathLoader;

        /*
        static string _documentsPath;
        public static string DocumentsPath
        {
            get
            {
                if (_documentsPath == null)
                    PlatformPathLoader?.Invoke();
                return _documentsPath;
            }
            set
            {
                _documentsPath = value;
            }
        }
        */

        static string _applicationDataPath;
        public static string ApplicationDataPath
        {
            get
            {
                if (_applicationDataPath == null)
                    PlatformPathLoader?.Invoke();
                return _applicationDataPath;
            }
            set
            {
                _applicationDataPath = value;
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);
            }
        }

        static string _applicationCachePath;
        public static string ApplicationCachePath
        {
            get
            {
                if (_applicationCachePath == null)
                    PlatformPathLoader?.Invoke();
                return _applicationCachePath;
            }
            set
            {
                _applicationCachePath = value;
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);
            }
        }

        static string _temporaryStoratePath;
        public static string TemporaryStoragePath
        {
            get
            {
                if (_temporaryStoratePath == null)
                    PlatformPathLoader?.Invoke();
                return _temporaryStoratePath;
            }
            set
            {
                _temporaryStoratePath = value;
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);
            }
        }

    }

    public class UnsupportedPlatformException : Exception
    {
        public UnsupportedPlatformException() : base("Unsupported Platform Exception") { }

        public UnsupportedPlatformException(string osType) : base("Unsupported Platform Exception: " + osType) { }
    }
}
