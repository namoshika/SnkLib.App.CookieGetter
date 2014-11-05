using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class Sleipnir3WekitBrowserManager : BlinkBrowserManager
    {
        public Sleipnir3WekitBrowserManager()
            : base("Sleipnir3 Wekit", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\ChromiumViewer", 2) { }
    }
}