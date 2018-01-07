using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var progress = new Progress<Tuple<int, float>>((fileCount) => 
            {
                Console.WriteLine("Files found: " + fileCount.Item1 + "\tFiles Scanned: " + fileCount.Item2  );
            });

            var files = DirectoryNFileSearch.DirectorySearch.FindFilesRegex(@"C:\", "*.txt", "Stack", progress);

            foreach (var f in files)
                Console.WriteLine(f);
        }
    }
}
