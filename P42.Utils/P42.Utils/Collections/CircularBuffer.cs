using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace P42.Utils
{
    public class CircularBuffer<T> : ICollection<T>
    {
        public int NextIndex { get; private set; }
        public int Count { get; private set; }

        public bool IsReadOnly => true;

        readonly public int Capacity;

        readonly T[] _buffer;
        readonly object _lock = new object();

        public CircularBuffer(int capacity)
        {
            Capacity = capacity;
            _buffer = new T[capacity];
        }

        public void Add(T item)
        {
            lock (_lock)
            {
                _buffer[NextIndex++] = item;
                Count++;
                if (Count > Capacity)
                    Count = Capacity;
                if (NextIndex >= Capacity)
                    NextIndex = 0;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                NextIndex = 0;
                Count = 0;
                for (int i = 0; i < Capacity; i++)
                    _buffer[i] = default;
            }
        }

        public bool Contains(T item)
        {
            return _buffer.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _buffer.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            var result = new T[Count];

            lock (_lock)
            {

                int index = 0;
                for (int i = NextIndex; i < Count; i++)
                    result[index++] = _buffer[i];
                for (int i = 0; i < NextIndex; i++)
                    result[index++] = _buffer[i];
            }

            return result.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var result = new T[Count];

            lock (_lock)
            {

                int index = 0;
                for (int i = NextIndex; i < Count; i++)
                    result[index++] = _buffer[i];
                for (int i = 0; i < NextIndex; i++)
                    result[index++] = _buffer[i];
            }

            return result.GetEnumerator();
        }
    }
}
