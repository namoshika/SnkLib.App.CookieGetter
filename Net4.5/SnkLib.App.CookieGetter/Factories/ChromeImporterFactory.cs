using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// ChromeからICookieImporterを取得します。
    /// </summary>
    public class ChromeImporterFactory : BlinkImporterFactory
    {
#pragma warning disable 1591
        public ChromeImporterFactory() : base("GoogleChrome", "%LOCALAPPDATA%\\Google\\Chrome\\User Data", 1) { }
#pragma warning restore 1591
    }
}