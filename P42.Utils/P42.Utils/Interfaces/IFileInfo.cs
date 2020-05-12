
namespace P42.Utils
{
	public interface IFileInfo : IFileSystemInfo
	{
		IDirectoryInfo Directory { get; }

		string DirectoryName { get; }

		bool IsReadOnly { get; set; }

		long Length { get; }

		IFileInfo CopyTo ( string destFileName, bool overwrite=false );

		void MoveTo ( string destFileName );


	}
}

