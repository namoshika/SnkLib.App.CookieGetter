using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class ComodoDragonBrowserManager : WebkitBrowserManager
    {
        public ComodoDragonBrowserManager()
            : base(inf => new BlinkCookieGetter(inf), "ComodoDragon", "%LOCALAPPDATA%\\Comodo\\Dragon\\User Data") { }
    }
}