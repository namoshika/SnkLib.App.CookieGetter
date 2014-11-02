using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class FirefoxBrowserManager : GeckoBrowserManager
    {
        public FirefoxBrowserManager() : base("Firefox", "%APPDATA%\\Mozilla\\Firefox") { }
    }
}