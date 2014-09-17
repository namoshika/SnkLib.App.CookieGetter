using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class ComodoDragonBrowserManager : WebkitBrowserManager
    {
        public ComodoDragonBrowserManager()
            : base(conf => new BlinkCookieGetter(conf), "ComodoDragon", "%LOCALAPPDATA%\\Comodo\\Dragon\\User Data") { }
    }
}