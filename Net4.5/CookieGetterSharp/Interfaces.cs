using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Hal.CookieGetterSharp
{
    public interface ICookieGetter
    {
        CookieStatus Status { get; }

        Cookie GetCookie(Uri url, string key);
        CookieCollection GetCookieCollection(Uri url);
        CookieContainer GetAllCookies();
    }
    public interface IBrowserManager
    {
        BrowserType BrowserType { get; }
        ICookieGetter CreateDefaultCookieGetter();
        ICookieGetter[] CreateCookieGetters();
    }
    public enum BrowserType
    {
#if NET20
        IE, IEComponent, IESafemode, Firefox3,
		GoogleChrome3, GoogleChromeplus, Opera10, Safari4,
        Lunascape5Gecko, Lunascape6Gecko, Lunascape6Webkit,
		Chromium, PaleMoon, Opera11Beta
#else
        IE, IEComponent, IESafemode, IEEPMode, Firefox,
        PaleMoon, Songbird, SeaMonkey, GoogleChrome, ComodoDragon,
        ComodoIceDragon, ChromePlus, CoolNovo, OperaWebkit,
        Opera, Opera64, Safari, LunascapeGecko, LunascapeWebkit,
        Sleipnir3Gecko, Sleipnir3Webkit, Sleipnir4Blink,
        RockMelt, Maxthon, Chromium, Sleipnir5Blink, TungstenBlink
#endif
    }
}
