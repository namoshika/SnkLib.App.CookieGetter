using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class TungstenBrowserManager : WebkitBrowserManager
    {
        public TungstenBrowserManager()
            : base(conf => new BlinkCookieGetter(conf), "TungstenBlink", "%APPDATA%\\Tungsten\\profile") { }
    }
}