using System;
using Android.OS;
using P42.Utils.Interfaces;

namespace P42.Utils.Droid
{
    public class DiskSpace : IDiskSpace
    {
        static Java.IO.File[] FileRoots => P42.Utils.Droid.Settings.Context.GetExternalFilesDirs(null);

        public static string PhoneRootPath => FileRoots.Length > 0 ? FileRoots[0].AbsolutePath : null; // PhoneMemory
        //public static string SDCardPath = FileRoots.Length > 1 ? FileRoots[1].AbsolutePath : null; // SCCard (if available)
        //public static string UsbPath = FileRoots.Length > 2 ? FileRoots[2].AbsolutePath : null; // USB (if available)

        public ulong Free => FreeMemory(PhoneRootPath);

        public ulong Size => TotalMemory(PhoneRootPath);

        public ulong Used => UsedMemory(PhoneRootPath);


        public static ulong TotalMemory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return 0;
            using (StatFs statFs = new StatFs(path))
            {
                if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.JellyBeanMr2) //Build.VERSION_CODES.JellyBeanMr2)
                                                                                      //noinspection deprecation
#pragma warning disable CS0618 // Type or member is obsolete
                    return (ulong)(statFs.BlockCount * statFs.BlockSize);
#pragma warning restore CS0618 // Type or member is obsolete
                return (ulong)(statFs.BlockCountLong * statFs.BlockSizeLong);
            }
        }

        public static ulong FreeMemory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return 0;
            using (StatFs statFs = new StatFs(path))
            {
                if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.JellyBeanMr2) //Build.VERSION_CODES.JellyBeanMr2)
                    //noinspection deprecation
#pragma warning disable CS0618 // Type or member is obsolete
                    return (ulong)(statFs.AvailableBlocks * statFs.BlockSize);
#pragma warning restore CS0618 // Type or member is obsolete
                return (ulong)(statFs.AvailableBlocksLong * statFs.BlockSizeLong);
            }
        }

        public static ulong UsedMemory(string path)
        {
            var total = TotalMemory(path);
            var free = FreeMemory(path);
            return total - free;
        }

    }
}
