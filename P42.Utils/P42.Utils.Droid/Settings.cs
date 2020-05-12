using System;
namespace P42.Utils.Droid
{
    public static class Settings
    {
        public static Android.Content.Context Context { get; private set; }


        public static void Init(Android.Content.Context context)
        {
            Context = context;
            P42.Utils.Environment.Init();
            P42.Utils.Environment.PlatformPathLoader = PlatformPathLoader;
            P42.Utils.DiskSpace.PlatformDiskSpace = new DiskSpace();
        }

        static void PlatformPathLoader()
        {
            //P42.Utils.Environment.DocumentsPath = Context.FilesDir.Path;
            P42.Utils.Environment.ApplicationDataPath = System.IO.Path.Combine(Context.ApplicationInfo.DataDir, "AppData");
            P42.Utils.Environment.ApplicationCachePath = Context.CacheDir.Path;
            P42.Utils.Environment.TemporaryStoragePath = System.IO.Path.Combine(Context.CacheDir.Path, "tmp");
        }
    }
}
