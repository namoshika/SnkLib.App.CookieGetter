using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class TungstenBrowserManager : BlinkBrowserManager
    {
        public TungstenBrowserManager() : base("TungstenBlink", "%APPDATA%\\Tungsten\\profile") { }
    }
}