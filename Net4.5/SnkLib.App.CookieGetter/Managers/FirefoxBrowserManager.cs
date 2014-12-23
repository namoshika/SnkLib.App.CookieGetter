using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// FirefoxからICookieImporterを取得します。
    /// </summary>
    public class FirefoxBrowserManager : GeckoBrowserManager
    {
#pragma warning disable 1591
        public FirefoxBrowserManager() : base("Firefox", "%APPDATA%\\Mozilla\\Firefox", 1) { }
#pragma warning restore 1591
    }
}