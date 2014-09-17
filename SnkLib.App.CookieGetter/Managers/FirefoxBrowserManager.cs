using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class FirefoxBrowserManager : GeckoBrowserManager
    {
        public FirefoxBrowserManager()
            : base(conf => new GeckoCookieGetter(conf), "Firefox", "%APPDATA%\\Mozilla\\Firefox") { }
    }
}