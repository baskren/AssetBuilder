using System;
namespace P42.Utils
{
	public class EventArgs<T> : EventArgs
	{
		public T Value { get; set; }

		public EventArgs(T value)
		{
			Value = value;
		}
	}
}

