using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if NETSTANDARD2_0
using System.IO.IsolatedStorage;
using System.IO;

namespace P42.Utils
{
    static class IsolatedStorageExtensions

    {



        /// <summary>
        /// Reads the contents of a file as a string
        /// </summary>
        /// <param name="file">The file to read </param>
        /// <returns>The contents of the file</returns>
        public static async Task<string> ReadAllTextAsync(this IsolatedStorageFile storage, string path)
        {
            if (!storage.FileExists(path))
                return null;

            using (var stream = new IsolatedStorageFileStream(path, FileMode.Open, storage))
            {
                await Task.Delay(5);
                using (var sr = new StreamReader(stream))
                {
                    string text = await sr.ReadToEndAsync();
                    return text;
                }
            }
        }

        /// <summary>
        /// Writes text to a file, overwriting any existing data
        /// </summary>
        /// <param name="file">The file to write to</param>
        /// <param name="contents">The content to write to the file</param>
        /// <returns>A task which completes when the write operation finishes</returns>
        public static async Task WriteAllTextAsync(this IsolatedStorageFile storage, string path, string contents)
        {
            var tempFileName = Guid.NewGuid().ToString();

            using (var stream = new IsolatedStorageFileStream(tempFileName, FileMode.Create, storage))
            {
                stream.SetLength(0);
                await Task.Delay(5);
                using (var sw = new StreamWriter(stream))
                {
                    await sw.WriteAsync(contents);
                }
            }

            storage.MoveFile(tempFileName, path);
        }

        public static string ReadAllText(this IsolatedStorageFile storage, string path)
        {
            Task<string> task = Task.Run(() => storage.ReadAllTextAsync(path));
            return task.Result;
        }

        public static void WriteAllText(this IsolatedStorageFile storage, string path, string contents)
        {
            var task = Task.Run(() => storage.WriteAllTextAsync(path, contents));
            task.Wait();
            return;
        }

        public static StreamReader StreamReader(this IsolatedStorageFile storage, string path)
        {
            var stream = new IsolatedStorageFileStream(path, FileMode.Open, storage);
            if (stream != null)
                return new StreamReader(stream);
            return null;
        }

        public static StreamWriter StreamWriter(this IsolatedStorageFile storage, string path)
        {
            var stream = new IsolatedStorageFileStream(path, FileMode.CreateNew, storage);
            if (stream != null)
                return new StreamWriter(stream);
            return null;
        }
    }
}
#endif
