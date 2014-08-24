using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class OperaWebkitBrowserManager : WebkitBrowserManager
    {
        public OperaWebkitBrowserManager()
            : base(inf => new BlinkCookieGetter(inf), "Opera Webkit",
            "%APPDATA%\\Opera Software\\Opera Stable", defaultFolder: string.Empty) { }
    }
}