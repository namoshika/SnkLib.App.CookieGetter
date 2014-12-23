using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// TungstenからICookieImporterを取得します。
    /// </summary>
    public class TungstenBrowserManager : BlinkBrowserManager
    {
#pragma warning disable 1591
        public TungstenBrowserManager() : base("TungstenBlink", "%APPDATA%\\Tungsten\\profile") { }
#pragma warning restore 1591
    }
}