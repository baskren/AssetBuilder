using System;
using System.Collections.Generic;

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

        public static string ToPascalCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static List<float> ToFloatList(this string text)
        {
            var result = new List<float>();
            text = text.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return result;
            var newText = text[0].ToString();
            for(int i=1;i<text.Length;i++)
            {
                if (text[i] == '-' && char.IsDigit(text[i - 1]))
                    newText += " ";
                newText += text[i];
            }
            var tokens = newText.Split(new char[] { ' ', ',' });

            foreach (var token in tokens)
            {
                if (float.TryParse(token, out float value))
                    result.Add(value);
            }
            return result;
        }

    }
}
