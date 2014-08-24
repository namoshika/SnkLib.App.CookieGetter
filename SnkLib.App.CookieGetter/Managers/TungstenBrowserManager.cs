using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class TungstenBrowserManager : WebkitBrowserManager
    {
        public TungstenBrowserManager()
            : base(inf => new BlinkCookieGetter(inf), "TungstenBlink", "%APPDATA%\\Tungsten\\profile") { }
    }
}