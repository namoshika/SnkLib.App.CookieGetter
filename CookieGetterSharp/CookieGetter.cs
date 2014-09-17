using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hal.CookieGetterSharp
{
    using SunokoLibrary.Application;
    using SunokoLibrary.Application.Browsers;

    [Obsolete("SunokoLibrary.Application.CookieGettersを使用してください。")]
    public class CookieGetter : ICookieGetter
    {
        public CookieGetter(ICookieImporter importer, IBrowserManager manager)
        {
            Importer = importer;
            Status = new CookieStatus(this, manager);
        }
        internal ICookieImporter Importer { get; set; }
        public CookieStatus Status { get; private set; }
        public Cookie GetCookie(Uri url, string key)
        { return GetCookieCollection(url)[key]; }
        public CookieCollection GetCookieCollection(Uri url)
        {
            try
            {
                var cookies = new CookieContainer();
                var res = Importer.GetCookiesAsync(url, cookies).Result;
                switch (res)
                {
                    case ImportResult.Success:
                    case ImportResult.Unavailable:
                        return cookies.GetCookies(url);
                    default:
                        throw new CookieImportException("Cookie取得に失敗しました。", res);
                }
            }
            catch (CookieImportException e)
            { throw new CookieGetterException(e); }
        }
        public CookieContainer GetAllCookies()
        { throw new NotImplementedException(); }
        public override string ToString()
        { return Status.ToString(); }
        public override bool Equals(object obj)
        {
            var that = obj as ICookieGetter;
            return that == null ? false : Status.Equals(that.Status);
        }
        public override int GetHashCode()
        { return Status.GetHashCode(); }

        public static Queue<Exception> Exceptions = new Queue<Exception>();
        static IBrowserManager[] _browserManagers;
        static CookieGetter()
        {
            _browserManagers = new IBrowserManager[]{
                new BrowserManager(BrowserType.IE, new IEBrowserManager()),
                new BrowserManager(BrowserType.Firefox, new FirefoxBrowserManager()),
                new BrowserManager(BrowserType.PaleMoon, new PaleMoonBrowserManager()),
                new BrowserManager(BrowserType.SeaMonkey, new SeaMonkeyBrowserManager()),
                new BrowserManager(BrowserType.GoogleChrome, new GoogleChromeBrowserManager()),
                new BrowserManager(BrowserType.ComodoDragon, new ComodoDragonBrowserManager()),
                new BrowserManager(BrowserType.ComodoIceDragon, new ComodoIceDragonBrowserManager()),
                new BrowserManager(BrowserType.OperaWebkit, new OperaWebkitBrowserManager()),
                new BrowserManager(BrowserType.LunascapeGecko, new LunascapeGeckoBrowserManager()),
                new BrowserManager(BrowserType.LunascapeWebkit, new LunascapeWebkitBrowserManager()),
                new BrowserManager(BrowserType.Sleipnir4Blink, new Sleipnir4BlinkBrowserManager()),
                new BrowserManager(BrowserType.Sleipnir5Blink, new Sleipnir5BlinkBrowserManager()),
                new BrowserManager(BrowserType.Chromium, new ChromiumBrowserManager()),
                new BrowserManager(BrowserType.CoolNovo, new CoolNovoBrowserManager()),
                new BrowserManager(BrowserType.Maxthon, new MaxthonBrowserManager()),
                new BrowserManager(BrowserType.TungstenBlink, new TungstenBrowserManager())
            };
        }

        public static ICookieGetter[] CreateInstances(bool availableOnly)
        {
            var results = new List<ICookieGetter>();
            foreach (var manager in _browserManagers)
            {
                if (availableOnly)
                {
                    foreach (var cg in manager.CreateCookieGetters())
                    {
                        if (cg.Status.IsAvailable)
                        {
                            results.Add(cg);
                        }
                    }
                }
                else
                {
                    results.AddRange(manager.CreateCookieGetters());
                }
            }

            return results.ToArray();
        }
        public static ICookieGetter CreateInstance(BrowserType type)
        {
            foreach (var manager in _browserManagers)
                if (manager.BrowserType == type)
                    return manager.CreateDefaultCookieGetter();
            return null;
        }
        public static ICookieGetter CreateInstance(CookieStatus status)
        {
            ICookieGetter cookieGetter = CreateInstance(status.BrowserType);
            cookieGetter.Status.Name = status.Name;
            cookieGetter.Status.CookiePath = status.CookiePath;
            cookieGetter.Status.DisplayName = status.DisplayName;

            return cookieGetter;
        }
    }
}