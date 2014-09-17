using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class Sleipnir5BlinkBrowserManager : WebkitBrowserManager
    {
        public Sleipnir5BlinkBrowserManager()
            : base(conf => new BlinkCookieGetter(conf),
            "Sleipnir5 Blink", "%APPDATA%\\Fenrir Inc\\Sleipnir5\\setting\\modules\\ChromiumViewer") { }
    }
}