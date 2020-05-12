using System;
using System.Collections.Generic;
using Android.OS;
using P42.Utils;

namespace P42.Utils
{
    /*
    public static class LogEventParameterExtensions
    {
        public static void Add(this Bundle bundle, string parameterName, LogEventParameterValue value)
        {
            if (value._type == typeof(string))
                bundle.PutString(parameterName, value._string);
            else if (value._type == typeof(char))
                bundle.PutChar(parameterName, value._char);
            else if (value._type == typeof(short))
                bundle.PutShort(parameterName, value._short);
            else if (value._type == typeof(int))
                bundle.PutInt(parameterName, value._int);
            else if (value._type == typeof(long))
                bundle.PutLong(parameterName, value._long);
            else if (value._type == typeof(float))
                bundle.PutFloat(parameterName, value._float);
            else
                bundle.PutDouble(parameterName, value._double);
        }

        public static Bundle ToBundle(this Dictionary<string, LogEventParameterValue> csDict)
        {
            var bundle = new Bundle();
            foreach (var pair in csDict)
                bundle.Add(pair.Key.ToAnalyticsEventOrParameter(), pair.Value);
            return bundle;
        }
    }
    */
}
