using System;
namespace AssetBuilder
{
    public static class StringExtensions
    {
        public static string SubstringWithTerminator(this string s, int startIndex, char terminator)
        {
            var terminatorIndex = s.IndexOf(terminator, startIndex);
            if (terminatorIndex > startIndex)
                return s.Substring(startIndex, terminatorIndex - startIndex);
            return null;
        }
    }
}
