using SunokoLibrary.Application.Browsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
    class PaleMoonImporterFactory : GeckoImporterFactory
    {
        public PaleMoonImporterFactory() : base("PaleMoon", "%APPDATA%\\Moonchild Productions\\Pale Moon") { }
    }
}