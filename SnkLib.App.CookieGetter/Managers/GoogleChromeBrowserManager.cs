using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class GoogleChromeBrowserManager : WebkitBrowserManager
    {
        public GoogleChromeBrowserManager()
            : base(conf => new BlinkCookieGetter(conf), "GoogleChrome", "%LOCALAPPDATA%\\Google\\Chrome\\User Data") { }
    }
}