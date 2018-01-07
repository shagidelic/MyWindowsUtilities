using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryNFileSearch
{
    public static class DirectorySearch
    {
        public static List<string> FindFilesRegex(string rootDir, string fileSearchPattern, string contentSearchPattern, 
            IProgress<Tuple<int, float, string>> progress, 
            bool searchTopOnly = false,
            int maxSearchableFileSizeInMB=10)
        {
            int maxFileSize = maxSearchableFileSizeInMB * 1024 * 1024;
            int filesScanned = 0;

            ConcurrentBag<string> foundFiles = new ConcurrentBag<string>();
            try
            {
                Parallel.ForEach(GetFiles(rootDir, fileSearchPattern, searchTopOnly), 
                    new ParallelOptions() { MaxDegreeOfParallelism = 100 }, 
                    file =>
                {
                    // File size check
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Length >= maxFileSize)
                        return;

                    //File content check
                    if (!string.IsNullOrEmpty(contentSearchPattern))
                    {
                        if (SearchContent(file, contentSearchPattern))
                        {
                            foundFiles.Add(file);
                            progress.Report(new Tuple<int, float, string>(foundFiles.Count, filesScanned, file));
                        }
                    }
                    else
                    {
                        foundFiles.Add(file);
                        progress.Report(new Tuple<int, float, string>(foundFiles.Count, filesScanned, file));
                    }

                    Interlocked.Increment(ref filesScanned); // increment                                        
                });


                return foundFiles.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static IEnumerable<string> GetFiles(string root, string searchPattern, bool searchTopOnly)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(root);

            while(pending.Count != 0)
            {
                var path = pending.Pop();
                string[] next = null;

                try
                {
                    next = Directory.GetFiles(path, searchPattern, (searchTopOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories));
                }
                catch {}
                if (next != null && next.Length != 0)
                    foreach (var file in next) yield return file;

                try
                {
                    next = Directory.GetDirectories(path);
                    foreach (var subdir in next) pending.Push(subdir);
                }
                catch{}
            }
        }

        private static bool SearchContent(string file, string contentSearchPattern)
        {
            // Read in READONLY MODE
            try
            {
                var inStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(inStream);
                var text = sr.ReadToEnd();

                return (Regex.IsMatch(text, contentSearchPattern));
            }
            catch 
            {
                return false;
            }
        }
    }
}
