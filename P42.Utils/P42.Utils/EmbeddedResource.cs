using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace P42.Utils
{
    // DO NOT MAKE PUBLIC.  THIS RUNS VERY SLOWLY ON UWP .NET TOOL CHAIN.  USE EmbeddedResourceCache.GetStreamAsync instead.
    public static class EmbeddedResource
    {
        public static System.IO.Stream GetStream(string resourceId, Assembly assembly = null)
        {
            assembly = assembly ?? Environment.EmbeddedResourceAssemblyResolver?.Invoke(resourceId);
            if (assembly == null)
                return null;
            // the following is very bad for UWP?
            //if (!Available(resourceId, assembly))
            //    return null;
            return assembly.GetManifestResourceStream(resourceId);
        }

        static readonly Dictionary<Assembly, List<string>> _resources = new Dictionary<Assembly, List<string>>();

        public static bool Available(string resourceId, Assembly assembly=null)
        {
            assembly = assembly ?? Environment.EmbeddedResourceAssemblyResolver?.Invoke(resourceId);
            if (assembly == null)
                return false;
            if (!_resources.ContainsKey(assembly))
                _resources[assembly] = new List<string>(assembly.GetManifestResourceNames());
            return _resources[assembly].Contains(resourceId);
        }

        /// <summary>
        /// Copies an embedded resource - returns true if successful
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="resourceId"></param>
        /// <param name="destinationPath">full path (including filename) to which to write embedded resource</param>
        /// <param name="overwrite">overwrite file at full path, if it exists?</param>
        /// <returns></returns>
        public static bool TryCopyResource(this Assembly assembly, string resourceId, string destinationPath, bool overwrite = false)
        {
            if (Available(resourceId, assembly))
            {
                using (var input = assembly.GetManifestResourceStream(resourceId))
                {
                    if (Directory.Exists(destinationPath))
                        return false;
                    if (File.Exists(destinationPath))
                    {
                        if (overwrite)
                            return false;
                        File.Delete(destinationPath);
                    }
                    using (var output = File.Open(destinationPath, FileMode.CreateNew))
                    {
                        byte[] buffer = new byte[32768];
                        while (true)
                        {
                            int read = input.Read(buffer, 0, buffer.Length);
                            if (read <= 0)
                                break;
                            output.Write(buffer, 0, read);
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
