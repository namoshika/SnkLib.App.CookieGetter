using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class SeaMonkeyBrowserManager : GeckoBrowserManager
    {
        public SeaMonkeyBrowserManager()
            : base(conf => new GeckoCookieGetter(conf), "SeaMonkey", "%APPDATA%\\Mozilla\\SeaMonkey") { }
    }
}
