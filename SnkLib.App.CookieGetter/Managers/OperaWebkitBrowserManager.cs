using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class OperaWebkitBrowserManager : WebkitBrowserManager
    {
        public OperaWebkitBrowserManager()
            : base(conf => new BlinkCookieGetter(conf), "Opera Webkit",
            "%APPDATA%\\Opera Software\\Opera Stable", defaultFolder: string.Empty) { }
    }
}