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
        IE, IEComponent, IESafemode, IEEPMode, Firefox,
        PaleMoon, Songbird, SeaMonkey, GoogleChrome, ComodoDragon,
        ComodoIceDragon, ChromePlus, CoolNovo, OperaWebkit,
        Opera, Opera64, Safari, LunascapeGecko, LunascapeWebkit,
        Sleipnir3Gecko, Sleipnir3Webkit, Sleipnir4Blink,
        RockMelt, Maxthon, Chromium, Sleipnir5Blink, TungstenBlink
    }
}
