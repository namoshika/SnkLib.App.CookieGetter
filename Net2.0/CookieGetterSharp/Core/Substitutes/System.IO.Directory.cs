using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    static class Directory
    {
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        { return System.IO.Directory.GetFiles(path, searchPattern); }
        public static IEnumerable<string> EnumerateDirectories(string path)
        { return System.IO.Directory.GetDirectories(path); }
		public static bool Exists(string path)
        { return System.IO.Directory.Exists(path); }
    }
}
