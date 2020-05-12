using System;
using System.Collections.Generic;

namespace P42.Utils
{
    public static class ICollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> range)
        {
            if (range != null && collection != null)
                foreach (var item in range)
                    collection.Add(item);
        }
    }
}
