using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class Sleipnir4BlinkBrowserManager : WebkitBrowserManager
    {
        public Sleipnir4BlinkBrowserManager()
            : base(inf => new BlinkCookieGetter(inf),
            "Sleipnir4 Blink", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\ChromiumViewer") { }
    }
}