using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    static partial class Path
    {
        public static char DirectorySeparatorChar { get { return System.IO.Path.DirectorySeparatorChar; } }
		public static string Combine(params string[] entries)
        {
            return string.Join(
                System.IO.Path.DirectorySeparatorChar.ToString(),
                entries.Where(path => !string.IsNullOrEmpty(path)).ToArray());
        }
		public static string GetTempFileName()
        { return System.IO.Path.GetTempFileName(); }
        public static string GetDirectoryName(string path)
        { return System.IO.Path.GetDirectoryName(path); }
        public static string GetFileName(string path)
        { return System.IO.Path.GetFileName(path); }
    }
}
