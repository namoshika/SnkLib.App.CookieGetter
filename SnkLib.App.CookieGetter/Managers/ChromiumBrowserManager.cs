using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class ChromiumBrowserManager : BlinkBrowserManager
    {
        public ChromiumBrowserManager()
            : base(conf => new BlinkCookieGetter(conf), "Chromium", "%LOCALAPPDATA%\\Chromium\\User Data\\") { }
    }
}