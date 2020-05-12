using System;
using System.Collections.Generic;

namespace P42.Utils
{
    public static class CopyingExtensions
    {
        public static List<T> DeepValueCopy<T>(this List<T> source)
        {
            if (source == null)
                return null;
            var result = new List<T>();
            foreach (var member in source)
                result.Add(member);
            return result;
        }

        public static List<T> DeepReferenceCopy<T>(this List<T> source) where T : ICopiable<T>, new()
        {
            if (source == null)
                return null;
            var result = new List<T>();
            foreach (var member in source)
                result.Add(member.Copy());
            return result;
        }

        public static T Copy<T>(this T source) where T : ICopiable<T>
        {
            //if (source == null)
            //	return default(T);
            //if (typeof(T).GetTypeInfo().IsValueType || typeof(T)==typeof(string))
            //	return source;
            //return (T)Activator.CreateInstance(typeof(T), new object[] { source });
            //var result = new T();
            var result = (T)Activator.CreateInstance(typeof(T));
            result.PropertiesFrom(source);
            return result;
        }
    }
}
