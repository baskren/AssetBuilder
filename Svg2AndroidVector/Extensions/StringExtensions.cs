using System;
using System.Collections.Generic;

namespace Svg2AndroidVector
{
    public static class StringExtensions
    {
        public static string[] Split(this string s, string separator)
        {
            var result = new List<string>();
            int start = 0;
            int i = -1;
            while (start < s.Length && (i = s.IndexOf("at")) > 0)
            {
                result.Add(s.Substring(start, i - start));
                start = i + 2;
                if (start < s.Length)
                    s = s.Substring(start);
                else
                    s = "";
            }
            if (s.Length > 0)
                result.Add(s);
            return result.ToArray();
        }
    }
}
