using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class ComodoIceDragonBrowserManager : GeckoBrowserManager
    {
        public ComodoIceDragonBrowserManager()
            : base(conf => new GeckoCookieGetter(conf), "ComodoIceDragon", "%APPDATA%\\Comodo\\IceDragon") { }
    }
}