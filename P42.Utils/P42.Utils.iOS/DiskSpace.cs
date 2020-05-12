using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;
using P42.Utils.Interfaces;

#if __IOS__
namespace P42.Utils.iOS
#elif __macOS__
namespace P42.Utils.macOS
#endif
{
    public class DiskSpace : IDiskSpace
    {
        public ulong Free
        {
            get
            {
                var attributes = NSFileManager.DefaultManager.GetFileSystemAttributes(Settings.AppDirectory);
                var freeSpace = attributes.FreeSize; //attributes[NSFileManager.SystemFreeSize] as Int64;
                return freeSpace;
            }
        }

        public ulong Size
        {
            get
            {
                var attributes = NSFileManager.DefaultManager.GetFileSystemAttributes(Settings.AppDirectory);
                var size = attributes.Size; //attributes[NSFileManager.SystemFreeSize] as Int64;
                return size;
            }
        }

        public ulong Used => Size - Free;
    }
}
