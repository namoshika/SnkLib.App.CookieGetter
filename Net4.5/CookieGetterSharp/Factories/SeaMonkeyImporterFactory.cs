using SunokoLibrary.Application.Browsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hal.CookieGetterSharp
{
    class SeaMonkeyImporterFactory : GeckoImporterFactory
    {
        public SeaMonkeyImporterFactory() : base("SeaMonkey", "%APPDATA%\\Mozilla\\SeaMonkey") { }
    }
}
