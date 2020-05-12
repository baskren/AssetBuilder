
namespace P42.Utils
{
	public interface IDirectoryInfo : IFileSystemInfo
	{
		IDirectoryInfo Get(string path);

		IDirectoryInfo Parent {
			get;
		}

		IDirectoryInfo Root {
			get;
		}

		IDirectoryInfo CreateSubdirectory(string path);

		void Delete(bool recursive);

		IDirectoryInfo[] GetDirectories ( string searchPattern=null);

		IFileInfo[] GetFiles ( string searchPattern=null);

		void MoveTo ( string destDirName );
	}
}

