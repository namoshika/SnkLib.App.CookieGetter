using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class ChromiumBrowserManager : WebkitBrowserManager
    {
        public ChromiumBrowserManager()
            : base(inf => new BlinkCookieGetter(inf), "Chromium", "%LOCALAPPDATA%\\Chromium\\User Data\\") { }
    }
}