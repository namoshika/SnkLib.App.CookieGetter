using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
#if !NET20
using System.Threading.Tasks;
#endif

namespace Hal.CookieGetterSharp
{
    using SunokoLibrary.Application;
    using SunokoLibrary.Application.Browsers;

    [Obsolete("SunokoLibrary.Application.CookieGettersを使用してください。")]
    public class CookieGetter : ICookieGetter
    {
        internal CookieGetter(ICookieImporter importer)
        {
            Importer = importer;
            Status = new CookieStatus(this, ConvertBrowserType(importer.SourceInfo.BrowserName));
        }
        internal ICookieImporter Importer { get; set; }
        public CookieStatus Status { get; private set; }
        public Cookie GetCookie(Uri url, string key)
        { return GetCookieCollection(url)[key]; }
        public CookieCollection GetCookieCollection(Uri url)
        {
            try
            {
                var res = Importer.GetCookiesAsync(url).Result;
                switch (res.Status)
                {
                    case CookieImportState.Success:
                        return res.Cookies;
                    case CookieImportState.Unavailable:
                        return new CookieCollection();
                    default:
                        throw new CookieImportException("Cookie取得に失敗しました。", res.Status);
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

        static CookieGetter()
        {
            //対応させていないブラウザ、派生ブラウザとして省かれているブラウザを対応させる
            _getters = new CookieGetters(
                true, new PaleMoonImporterFactory(), new SeaMonkeyImporterFactory());

            //コア部分にはBrowserTypeが無いため、ブラウザ名とBrowserTypeの対応関係を
            //定義してBrowserType値の生成に使うする。
            _browserTypeDict = new Dictionary<string, BrowserType>() {
                {"IE Normal",  BrowserType.IE},
                {"IE Protected",  BrowserType.IESafemode},
                {"IE Enhanced Protected",  BrowserType.IEEPMode},
                {"Firefox",  BrowserType.Firefox},
                {"PaleMoon", BrowserType.PaleMoon},
                {"SeaMonkey", BrowserType.SeaMonkey},
                {"GoogleChrome",  BrowserType.GoogleChrome},
                {"IceDragon", BrowserType.ComodoIceDragon},
                {"Dragon", BrowserType.ComodoDragon},
                {"CoolNovo", BrowserType.CoolNovo},
                {"Opera Webkit",  BrowserType.OperaWebkit},
                {"Lunascape Gecko",  BrowserType.LunascapeGecko},
                {"Lunascape Webkit",  BrowserType.LunascapeWebkit},
                {"Sleipnir3 Gecko",  BrowserType.Sleipnir3Gecko},
                {"Sleipnir3 Wekit",  BrowserType.Sleipnir3Webkit},
                {"Sleipnir5 Blink",  BrowserType.Sleipnir5Blink},
                {"Chromium",  BrowserType.Chromium},
                {"Maxthon webkit",  BrowserType.Maxthon},
                {"TungstenBlink",  BrowserType.TungstenBlink},
            };
            _equivalentTypeDict = new Dictionary<BrowserType, BrowserType>() {
                { BrowserType.IEComponent, BrowserType.IE },
                { BrowserType.Sleipnir4Blink, BrowserType.Sleipnir5Blink },
            };
        }
        public static Queue<Exception> Exceptions = new Queue<Exception>();
        static int _browserTypeLen = Enum.GetNames(typeof(BrowserType)).Length;
        static Dictionary<string, BrowserType> _browserTypeDict;
        static Dictionary<BrowserType, BrowserType> _equivalentTypeDict;
        static ICookieImporterManager _getters;

        public static ICookieGetter[] CreateInstances(bool availableOnly)
        {
            var getters = _getters.GetInstancesAsync(availableOnly)
                .ContinueWith(tsk => tsk.Result.Select(importer => new CookieGetter(importer)))
                .Result.ToArray();
            return getters;
        }
        public static ICookieGetter CreateInstance(BrowserType type)
        {
            if (_equivalentTypeDict.ContainsKey(type))
                type = _equivalentTypeDict[type];
            var res = CreateInstances(false)
                .Where(getters => getters.Status.BrowserType == type)
                .FirstOrDefault();
            return res;
        }
        public static ICookieGetter CreateInstance(CookieStatus status)
        {
            ICookieGetter cookieGetter = CreateInstance(status.BrowserType);
            cookieGetter.Status.Name = status.Name;
            cookieGetter.Status.CookiePath = status.CookiePath;
            cookieGetter.Status.DisplayName = status.DisplayName;

            return cookieGetter;
        }
        static BrowserType ConvertBrowserType(string browserName)
        {
            BrowserType res;
            return _browserTypeDict.TryGetValue(browserName, out res)
                ? res : _browserTypeDict[browserName] = (BrowserType)_browserTypeLen++;
        }
    }
}