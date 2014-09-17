using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class Sleipnir3WekitBrowserManager : WebkitBrowserManager
    {
        public Sleipnir3WekitBrowserManager()
            : base(conf => new BlinkCookieGetter(conf),
            "Sleipnir3 Wekit", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\ChromiumViewer") { }
    }
}