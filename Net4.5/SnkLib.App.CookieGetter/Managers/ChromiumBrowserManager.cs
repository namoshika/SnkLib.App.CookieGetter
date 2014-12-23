using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// ChromiumからICookieImporterを取得します。
    /// </summary>
    public class ChromiumBrowserManager : BlinkBrowserManager
    {
#pragma warning disable 1591
        public ChromiumBrowserManager() : base("Chromium", "%LOCALAPPDATA%\\Chromium\\User Data\\") { }
#pragma warning restore 1591
    }
}