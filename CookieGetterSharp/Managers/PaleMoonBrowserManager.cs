using SunokoLibrary.Application.Browsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
    class PaleMoonBrowserManager : GeckoBrowserManager
    {
        public PaleMoonBrowserManager() : base("PaleMoon", "%APPDATA%\\Moonchild Productions\\Pale Moon", 2) { }
    }
}