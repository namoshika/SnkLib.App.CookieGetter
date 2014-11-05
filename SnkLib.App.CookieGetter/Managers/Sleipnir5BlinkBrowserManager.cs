using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class Sleipnir5BlinkBrowserManager : BlinkBrowserManager
    {
        public Sleipnir5BlinkBrowserManager()
            : base("Sleipnir5 Blink", "%APPDATA%\\Fenrir Inc\\Sleipnir5\\setting\\modules\\ChromiumViewer", 2) { }
    }
}