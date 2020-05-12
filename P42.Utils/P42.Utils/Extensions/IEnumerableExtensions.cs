using System;
using System.Collections;
using System.Text;

namespace P42.Utils
{
    public static class IEnumerableExtensions
    {
        public static bool Contains(this IEnumerable enumerable, object obj)
        {
            foreach (var item in enumerable)
            {
                if (item is IEnumerable subEnumerable)
                {
                    if (subEnumerable.Contains(obj))
                        return true;
                }
                if (item.Equals(obj))
                    return true;
            }
            return false;
        }
    }
}
