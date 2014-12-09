using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class ChromiumBrowserManager : BlinkBrowserManager
    {
        public ChromiumBrowserManager() : base("Chromium", "%LOCALAPPDATA%\\Chromium\\User Data\\") { }
    }
}