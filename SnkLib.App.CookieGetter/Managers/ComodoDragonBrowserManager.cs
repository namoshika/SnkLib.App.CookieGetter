using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class ComodoDragonBrowserManager : BlinkBrowserManager
    {
        public ComodoDragonBrowserManager()
            : base(conf => new BlinkCookieGetter(conf), "ComodoDragon", "%LOCALAPPDATA%\\Comodo\\Dragon\\User Data") { }
    }
}