using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace P42.Utils
{
    public static class DictionaryExtensions
    {
        public static bool ContainsKeyThatContains<TValue>(this IDictionary<string, TValue> dictionary, string subkey)
        {
            var keys = dictionary.Keys;
            foreach (var key in keys)
                if (key.Contains(subkey))
                    return true;
            return false;
        }

        public static List<string> KeysThatContain<TValue>(this IDictionary<string, TValue> dictionary, string subkey)
        {
            var result = new List<string>();
            var keys = dictionary.Keys;
            foreach (var key in keys)
                if (key.Contains(subkey))
                    result.Add(key);
            if (keys.Count > 0)
                return result;
            return null;
        }

        public static List<TValue> ItemsWithKeysThatContain<TValue>(this IDictionary<string, TValue> dictionary, string subkey)
        {
            var result = new List<TValue>();
            var keys = dictionary.Keys;
            foreach (var key in keys)
                if (key.Contains(subkey))
                    result.Add(dictionary[key]);
            if (keys.Count > 0)
                return result;
            return null;
        }

        public static bool TryGetKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, out TKey key)
        {
            foreach (var kvp in dictionary)
            {
                if (Equals(value, kvp.Value))
                {
                    key = kvp.Key;
                    return true;
                }
            }
            key = default;
            return false;
        }
    }
}
