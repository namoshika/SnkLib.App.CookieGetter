using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class FirefoxBrowserManager : GeckoBrowserManager
    {
        public FirefoxBrowserManager()
            : base(inf => new GeckoCookieGetter(inf), "Firefox", "%APPDATA%\\Mozilla\\Firefox") { }
    }
}