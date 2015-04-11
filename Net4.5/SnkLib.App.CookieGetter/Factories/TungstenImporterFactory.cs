using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// TungstenからICookieImporterを取得します。
    /// </summary>
    public class TungstenImporterFactory : BlinkImporterFactory
    {
#pragma warning disable 1591
        public TungstenImporterFactory() : base("Tungsten Blink", "%APPDATA%\\Tungsten\\profile") { }
#pragma warning restore 1591
    }
}