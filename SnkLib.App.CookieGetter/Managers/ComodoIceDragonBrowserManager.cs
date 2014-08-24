using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class ComodoIceDragonBrowserManager : GeckoBrowserManager
    {
        public ComodoIceDragonBrowserManager()
            : base(inf => new GeckoCookieGetter(inf), "ComodoIceDragon", "%APPDATA%\\Comodo\\IceDragon") { }
    }
}