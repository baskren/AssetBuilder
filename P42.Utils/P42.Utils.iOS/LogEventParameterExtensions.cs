using System;
using Foundation;
using System.Collections.Generic;
namespace P42.Utils
{
    /*
    public static class LogEventParameterExtensions
    {
        public static NSObject ToNSObject(this LogEventParameterValue value)
        {
            if (value._type == typeof(string))
                return new NSString(value._string ?? "");
            if (value._type == typeof(char))
                return new NSNumber(value._char);
            if (value._type == typeof(short))
                return new NSNumber(value._short);
            if (value._type == typeof(int))
                return new NSNumber(value._int);
            if (value._type == typeof(long))
                return new NSNumber(value._long);
            if (value._type == typeof(float))
                return new NSNumber(value._float);
            return new NSNumber(value._double);
        }

        public static NSDictionary<NSString, NSObject> ToNSDictionary(this Dictionary<string, LogEventParameterValue> csDict)
        {
            var nsDict = new NSMutableDictionary<NSString, NSObject>();
            foreach (var pair in csDict)
            {
                var key = pair.Key.ToAnalyticsEventOrParameter();
                var value = pair.Value.ToNSObject();
                nsDict[key] = value;
            }
            var result = new NSDictionary<NSString, NSObject>(nsDict.Keys, nsDict.Values);
            return result;
        }
    }
    */
}
