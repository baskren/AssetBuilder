using System;
namespace AndroidVector
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;
            return char.ToLower(s[0]) + s.Substring(1);
        }
    }
}
