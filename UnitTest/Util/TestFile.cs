using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Util
{
    public static class TestFile
    {
        public static void WriteAllText(string file, string text)
        {
            var directory = Path.GetDirectoryName(file);
            var resultDirectory = Path.Combine(directory, "results");
            var nextResultCount = GetLastTestCount(resultDirectory) + 1;
            var fileInfo = new FileInfo(Path.Combine(directory, nextResultCount.ToString(), Path.GetFileName(file)));
            fileInfo.Directory.Create();
            File.WriteAllText(fileInfo.FullName, text);
        }

        public static int GetLastTestCount(string directory)
        {
            var info = new DirectoryInfo(directory);
            return info.Exists ? info.GetDirectories().Count() - 1 : -1;
        }
    }
}
