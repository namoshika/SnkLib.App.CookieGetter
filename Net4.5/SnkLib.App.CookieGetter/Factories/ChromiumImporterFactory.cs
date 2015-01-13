using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// ChromiumからICookieImporterを取得します。
    /// </summary>
    public class ChromiumImporterFactory : BlinkImporterFactory
    {
#pragma warning disable 1591
        public ChromiumImporterFactory() : base("Chromium", "%LOCALAPPDATA%\\Chromium\\User Data\\") { }
#pragma warning restore 1591
    }
}