using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class Sleipnir4BlinkBrowserManager : WebkitBrowserManager
    {
        public Sleipnir4BlinkBrowserManager()
            : base(conf => new BlinkCookieGetter(conf),
            "Sleipnir4 Blink", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\ChromiumViewer") { }
    }
}