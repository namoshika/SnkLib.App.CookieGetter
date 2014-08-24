using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class Sleipnir3GeckoBrowserManager : GeckoBrowserManager
    {
        public Sleipnir3GeckoBrowserManager()
            : base(inf => new GeckoCookieGetter(inf),
            "Sleipnir3 Gecko", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\geckoviewer") { }
    }
}