using System;
using System.Reflection;
namespace P42.Utils.Extensions
{
    public static class AssemblyExtensions
    {
        public static bool EmbeddedResourceExists(this Assembly assembly, string resourceId)
        {
            foreach (var res in assembly.GetManifestResourceNames())
                if (res == resourceId)
                    return true;
            return false;
        }
    }
}
