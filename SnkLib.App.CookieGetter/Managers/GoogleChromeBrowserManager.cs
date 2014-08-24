using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class GoogleChromeBrowserManager : WebkitBrowserManager
    {
        public GoogleChromeBrowserManager()
            : base(inf => new BlinkCookieGetter(inf), "GoogleChrome", "%LOCALAPPDATA%\\Google\\Chrome\\User Data") { }
    }
}