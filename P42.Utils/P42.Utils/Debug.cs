using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace P42.Utils
{
    public static class Debug
    {
        static Debug()
        {
            System.Diagnostics.Debug.IndentSize = 4;
        }

        public static Func<object, bool> ConditionFunc;

        public static bool IsMessagesEnabled = false;

        public static bool IsCensusEnabled = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void Message(object obj, string message)
        {
            if (IsMessagesEnabled && (ConditionFunc?.Invoke(obj) ?? false))
            {
                var method = new StackTrace().GetFrame(1).GetMethod();
                //var className = callingMethod.ReflectedType.Name;
                Message(message, method.ReflectedType + "." + method.Name);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void Message(string message, string callingMethod = null)
        {
            if (!IsMessagesEnabled)
                return;

            if (string.IsNullOrWhiteSpace(callingMethod))
            {
                var method = new StackTrace().GetFrame(1).GetMethod();
                callingMethod = method.ReflectedType + "." + method.Name;
            }
            System.Diagnostics.Debug.IndentSize = 4;
            if (message?.Contains("ENTER") ?? false)
            {
                if (System.Diagnostics.Debug.IndentLevel == 0)
                    System.Diagnostics.Debug.WriteLine("=========================================================");
            }
            if (message?.Contains("EXIT") ?? false)
            {
                System.Diagnostics.Debug.Unindent();
            }
            System.Diagnostics.Debug.WriteLine(callingMethod + ": " + message);
            if (message?.Contains("ENTER") ?? false)
                System.Diagnostics.Debug.Indent();
            if (message?.Contains("EXIT") ?? false)
            {
                if (System.Diagnostics.Debug.IndentLevel == 0)
                    System.Diagnostics.Debug.WriteLine("=========================================================");
            }
        }


        static string NameOfCallingClass()
        {
            string fullName;
            Type declaringType;
            int skipFrames = 2;
            do
            {
                MethodBase method = new StackFrame(skipFrames, false).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    return method.Name;
                }
                skipFrames++;
                fullName = declaringType.FullName;
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return fullName;
        }


        public static System.Collections.Concurrent.ConcurrentDictionary<Type, long> Census = new System.Collections.Concurrent.ConcurrentDictionary<Type, long>();

        public static void AddToCensus(this object obj)
        {
            if (IsCensusEnabled)
            {
                var type = obj.GetType();
                if (Census.TryGetValue(type, out long count))
                    Census[type] = count + 1;
                else
                    Census[type] = 1;
            }
        }

        public static void RemoveFromCensus(this object obj)
        {
            if (IsCensusEnabled)
            {
                var type = obj.GetType();
                if (Census.TryGetValue(type, out long count))
                    Census[type] = count - 1;
                else
                    System.Diagnostics.Debug.WriteLine("Debug." + P42.Utils.ReflectionExtensions.CallerString() + ":" + "TYPE NOT FOUND!!!! [" + type + "]");
            }
        }

        public static long CensusActiveCount
        {
            get
            {
                var actives = Census.Values.ToArray();
                return actives.Sum();
            }
        }
    }
}
