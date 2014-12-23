using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class OperaWebkitBrowserManager : BlinkBrowserManager
    {
        public OperaWebkitBrowserManager()
            : base("Opera Webkit", "%APPDATA%\\Opera Software\\Opera Stable", defaultFolder: string.Empty) { }
    }
}