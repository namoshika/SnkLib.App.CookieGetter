using SunokoLibrary.Application.Browsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
    class SeaMonkeyBrowserManager : GeckoBrowserManager
    {
        public SeaMonkeyBrowserManager() : base("SeaMonkey", "%APPDATA%\\Mozilla\\SeaMonkey") { }
    }
}
