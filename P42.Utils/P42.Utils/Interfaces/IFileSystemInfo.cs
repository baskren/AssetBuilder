
namespace P42.Utils
{
	public interface IFileSystemInfo
	{
		bool Exists {
			get;
		}

		string Extension {
			get;
		}

		string FullName {
			get;
		}

		string Name {
			get;
		}

		void Delete();

		int GetHashCode();

		string ToString();

		bool Equals(object obj);
	}
}

