using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// SleipnirからICookieImporterを取得します。
    /// </summary>
    public class SleipnirBrowserManager : ICookieImporterFactory
    {
#pragma warning disable 1591
        public IEnumerable<ICookieImporter> GetCookieImporters()
        {
            var importers =
                new[]{
                    _pnir3GeckoBrowserManager.GetCookieImporters(),
                    _pnir3BlinkBrowserManager.GetCookieImporters(),
                    _pnir5BlinkBrowserManager.GetCookieImporters(),
                };
            return importers.SelectMany(item => item);
        }
#pragma warning restore 1591

        static readonly Sleipnir3GeckoBrowserManager _pnir3GeckoBrowserManager = new Sleipnir3GeckoBrowserManager();
        static readonly Sleipnir3WekitBrowserManager _pnir3BlinkBrowserManager = new Sleipnir3WekitBrowserManager();
        static readonly Sleipnir5BlinkBrowserManager _pnir5BlinkBrowserManager = new Sleipnir5BlinkBrowserManager();

        class Sleipnir3GeckoBrowserManager : GeckoBrowserManager
        {
            public Sleipnir3GeckoBrowserManager()
                : base("Sleipnir3 Gecko", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\geckoviewer", 2) { }
        }
        class Sleipnir3WekitBrowserManager : BlinkBrowserManager
        {
            public Sleipnir3WekitBrowserManager()
                : base("Sleipnir3 Wekit", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\ChromiumViewer", 2) { }
        }
        class Sleipnir5BlinkBrowserManager : BlinkBrowserManager
        {
            public Sleipnir5BlinkBrowserManager()
                : base("Sleipnir5 Blink", "%APPDATA%\\Fenrir Inc\\Sleipnir5\\setting\\modules\\ChromiumViewer", 2) { }
        }
    }
}