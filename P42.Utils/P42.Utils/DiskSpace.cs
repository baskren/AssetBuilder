using System;
using P42.Utils.Interfaces;

namespace P42.Utils
{
    public static class DiskSpace
    {
        internal static IDiskSpace PlatformDiskSpace;

        public static ulong Free => PlatformDiskSpace?.Free ?? 0;

        public static ulong Size => PlatformDiskSpace?.Size ?? 0;

        public static ulong Used => PlatformDiskSpace?.Used ?? 0;

    }
}
