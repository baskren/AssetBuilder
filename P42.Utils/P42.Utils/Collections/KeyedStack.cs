using System;
using System.Collections.Generic;
namespace P42.Utils
{
    public class KeyedStack<K, V> : Dictionary<K, Stack<V>> where V : class
    {
        public KeyedStack()
        {
        }

        public void Push(K key, V value)
        {
            if (!ContainsKey(key))
                Add(key, new Stack<V>());
            this[key].Push(value);
        }

        public V Pop(K key)
        {
            if (!ContainsKey(key))
                return null;
            var stack = this[key];
            if (stack.Count > 0)
                return stack.Pop();
            return null;
        }

        public new int Count(K key)
        {
            if (!ContainsKey(key))
                return 0;
            return this[key].Count;
        }
    }
}
