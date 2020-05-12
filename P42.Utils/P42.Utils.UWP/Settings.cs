using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P42.Utils.UWP
{
	public static class Settings
	{
		public static Windows.UI.Xaml.Application Application { get; private set; }

		public static void Init(Windows.UI.Xaml.Application application)
		{
			Application = application;
			P42.Utils.Environment.Init();
			P42.Utils.Environment.PlatformPathLoader = PlatformPathLoader;
			P42.Utils.DiskSpace.PlatformDiskSpace = new DiskSpace();
			P42.Utils.Process.PlatformProcess = new P42.Utils.UWP.Process();
		}

		static void PlatformPathLoader()
		{
			/*
            //var envVars = System.Environment.GetEnvironmentVariables();

            try
            {
                //var documentsFolderPath = Windows.Storage.KnownFolders.DocumentsLibrary?.Path;
                //P42.Utils.Environment.DocumentsPath = documentsFolderPath;//Windows.Storage.ApplicationData.Current.LocalFolder.Path; 
            }
            catch (System.UnauthorizedAccessException)
            {
                // we don't have access.  Oh well.
            }
            */
			P42.Utils.Environment.ApplicationDataPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
			P42.Utils.Environment.ApplicationCachePath = Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path;
			P42.Utils.Environment.TemporaryStoragePath = Windows.Storage.ApplicationData.Current.TemporaryFolder.Path;
		}
	}
}
