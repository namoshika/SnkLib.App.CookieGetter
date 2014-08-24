using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class Sleipnir3WekitBrowserManager : WebkitBrowserManager
    {
        public Sleipnir3WekitBrowserManager()
            : base(inf => new BlinkCookieGetter(inf),
            "Sleipnir3 Wekit", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\ChromiumViewer") { }
    }
}