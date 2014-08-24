using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class PaleMoonBrowserManager : GeckoBrowserManager
    {
        public PaleMoonBrowserManager()
            : base(inf => new GeckoCookieGetter(inf), "PaleMoon", "%APPDATA%\\Moonchild Productions\\Pale Moon") { }
    }
}