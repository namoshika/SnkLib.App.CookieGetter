using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// SleipnirからICookieImporterを取得します。
    /// </summary>
    public class SleipnirImporterFactory : ComplexImporterFactory
    {
#pragma warning disable 1591
        public SleipnirImporterFactory()
            : base(new ICookieImporterFactory[] {
                new Sleipnir3GeckoBrowserManager(),
                new Sleipnir3WekitBrowserManager(),
                new Sleipnir5BlinkBrowserManager(),
            }) { }
#pragma warning restore 1591

        class Sleipnir3GeckoBrowserManager : GeckoImporterFactory
        {
            public Sleipnir3GeckoBrowserManager()
                : base("Sleipnir3 Gecko", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\geckoviewer", 2) { }
        }
        class Sleipnir3WekitBrowserManager : BlinkImporterFactory
        {
            public Sleipnir3WekitBrowserManager()
                : base("Sleipnir3 Wekit", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\ChromiumViewer", 2) { }
        }
        class Sleipnir5BlinkBrowserManager : BlinkImporterFactory
        {
            public Sleipnir5BlinkBrowserManager()
                : base("Sleipnir5 Blink", "%APPDATA%\\Fenrir Inc\\Sleipnir5\\setting\\modules\\ChromiumViewer", 2) { }
        }
    }
}