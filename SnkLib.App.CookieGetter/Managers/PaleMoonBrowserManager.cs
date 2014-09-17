using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class PaleMoonBrowserManager : GeckoBrowserManager
    {
        public PaleMoonBrowserManager()
            : base(conf => new GeckoCookieGetter(conf), "PaleMoon", "%APPDATA%\\Moonchild Productions\\Pale Moon") { }
    }
}