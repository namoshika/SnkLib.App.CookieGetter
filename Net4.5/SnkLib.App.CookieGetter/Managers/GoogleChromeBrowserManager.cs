using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// ChromeからICookieImporterを取得します。
    /// </summary>
    public class GoogleChromeBrowserManager : BlinkBrowserManager
    {
#pragma warning disable 1591
        public GoogleChromeBrowserManager() : base("GoogleChrome", "%LOCALAPPDATA%\\Google\\Chrome\\User Data", 1) { }
#pragma warning restore 1591
    }
}