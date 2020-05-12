using System;
namespace P42.Utils
{
    /*
    public static class AnalyticsStringExtensions
    {

        public static string ToAnalyticsEventOrParameter(this string s)
        {
            if (!char.IsLetterOrDigit(s, 0))
                s = s.Substring(1);
            if (!char.IsLetterOrDigit(s, s.Length - 1))
                s = s.Substring(0, s.Length - 1);
            //s = s.Replace("|", "ǀ");
            //s = s.Replace("৹", "ᆞ");
            s = s.Replace(".", "ᆞ");
            s = s.Replace(" ", "");
            s = s.SubstringLast(40);
            return s;
        }


        public static string ParameterPrepend(this string s, string p) => p != null ? p + "ᆞ" + s : s;


    }
*/
}
