using System;
using Foundation;

#if __IOS__
namespace P42.Utils.iOS
#elif __macOS__
namespace P42.Utils.macOS
#endif
{
    public static class Settings
    {
        public static void Init()
        {
            P42.Utils.Environment.Init();
            P42.Utils.Environment.PlatformPathLoader = PlatformPathLoader;
            P42.Utils.DiskSpace.PlatformDiskSpace = new DiskSpace();
        }

        static void PlatformPathLoader()
        {
            //var documentsDir = NSFileManager.DefaultManager.GetUrl(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, null, true, out nsError);
            //if (nsError == null && !string.IsNullOrWhiteSpace(documentsDir?.Path))
            //    P42.Utils.Environment.DocumentsPath = documentsDir.Path;
            //else
            //    throw new Exception("Could not get iOS Documents Directory");

            var appSupportDir = NSFileManager.DefaultManager.GetUrl(NSSearchPathDirectory.ApplicationSupportDirectory, NSSearchPathDomain.User, null, true, out NSError nsError);
            if (nsError == null && !string.IsNullOrWhiteSpace(appSupportDir?.Path))
                P42.Utils.Environment.ApplicationDataPath = System.IO.Path.Combine(appSupportDir.Path, NSBundle.MainBundle.BundleIdentifier);
            else
                throw new Exception("Could not get iOS Application Support Directory");


            var cacheDir = NSFileManager.DefaultManager.GetUrl(NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User, null, true, out nsError);
            if (nsError == null && !string.IsNullOrWhiteSpace(cacheDir?.Path))
                P42.Utils.Environment.ApplicationCachePath = System.IO.Path.Combine(cacheDir.Path, NSBundle.MainBundle.BundleIdentifier);
            else
                throw new Exception("Could not get iOS Cache Directory");

            // either of the following seems to work
            //var tmpDirPath = NSFileManager.DefaultManager.GetTemporaryDirectory()?.Path;
            var tmpDirPath = System.IO.Path.GetTempPath();
            P42.Utils.Environment.TemporaryStoragePath = tmpDirPath;


            //var current = NSFileManager.DefaultManager.CurrentDirectory;

            nsError?.Dispose();
        }

        [System.Runtime.InteropServices.DllImport(ObjCRuntime.Constants.FoundationLibrary)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible", Justification = "<Pending>")]
        public static extern IntPtr NSHomeDirectory();

        static string _appDirectory;
        public static string AppDirectory
        {
            get
            {
                _appDirectory = _appDirectory ?? ((NSString)ObjCRuntime.Runtime.GetNSObject(NSHomeDirectory())).ToString();
                return _appDirectory;
            }
        }


    }
}
