using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class GoogleChromeBrowserManager : BlinkBrowserManager
    {
        public GoogleChromeBrowserManager() : base("GoogleChrome", "%LOCALAPPDATA%\\Google\\Chrome\\User Data", 1) { }
    }
}