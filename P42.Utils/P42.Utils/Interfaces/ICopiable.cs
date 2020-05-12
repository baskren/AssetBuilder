using System;
namespace P42.Utils
{
	public interface ICopiable<T>
	{
		void PropertiesFrom(T other);
	}
}
