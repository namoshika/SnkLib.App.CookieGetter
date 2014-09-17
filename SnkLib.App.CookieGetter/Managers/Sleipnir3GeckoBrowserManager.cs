using System;
using System.Collections.Generic;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    public class Sleipnir3GeckoBrowserManager : GeckoBrowserManager
    {
        public Sleipnir3GeckoBrowserManager()
            : base(conf => new GeckoCookieGetter(conf),
            "Sleipnir3 Gecko", "%APPDATA%\\Fenrir Inc\\Sleipnir\\setting\\modules\\geckoviewer") { }
    }
}